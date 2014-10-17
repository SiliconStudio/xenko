// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Checks if an effect has already been compiled in its cache before deferring to a real <see cref="IEffectCompiler"/>.
    /// </summary>
    [DataSerializerGlobal(null, typeof(KeyValuePair<HashSourceCollection, EffectBytecode>))]
    public class EffectCompilerCache : EffectCompilerChain
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("EffectCompilerCache");
        private readonly Dictionary<ObjectId, EffectBytecode> storedResults = new Dictionary<ObjectId, EffectBytecode>();
        private const string CompiledShadersKey = "__shaders_bytecode__";

        public EffectCompilerCache(EffectCompilerBase compiler) : base(compiler)
        {
        }

        public override EffectBytecode Compile(ShaderMixinSource mixin, string fullEffectName, ShaderMixinParameters compilerParameters, HashSet<string> modifiedShaders, HashSet<string> recentlyModifiedShaders, LoggerResult log)
        {
            var database = AssetManager.FileProvider;
            if (database == null)
            {
                throw new NotSupportedException("Using the cache requires to AssetManager.FileProvider to be valid.");
            }

            // remove the old shaders
            if (recentlyModifiedShaders != null && recentlyModifiedShaders.Count != 0)
                RemoveObsoleteStoredResults(recentlyModifiedShaders);

            var ids = ShaderMixinObjectId.Compute(mixin, compilerParameters);

            EffectBytecode bytecode = null;
            lock (storedResults)
            {
                if (storedResults.TryGetValue(ids.FullParametersId, out bytecode))
                {
                    return bytecode;
                }

                // Final url of the compiled bytecode
                var compiledUrl = string.Format("{0}/{1}", CompiledShadersKey, ids.CompileParametersId);

                // ------------------------------------------------------------------------------------------------------------
                // 1) Try to load latest bytecode
                // ------------------------------------------------------------------------------------------------------------
                ObjectId bytecodeId;
                if (database.AssetIndexMap.TryGetValue(compiledUrl, out bytecodeId))
                {
                    using (var stream = database.ObjectDatabase.OpenStream(bytecodeId))
                    {
                        var localBytecode = BinarySerialization.Read<EffectBytecode>(stream);

                        // If latest bytecode is in sync
                        if (!Platform.IsWindowsDesktop || CheckBytecodeInSyncAgainstSources(localBytecode, database))
                        {
                            bytecode = localBytecode;

                            // if bytecode contains a modified shource, do not use it.
                            if (modifiedShaders != null && modifiedShaders.Count != 0 && IsBytecodeObsolete(bytecode, modifiedShaders))
                                bytecode = null;
                        }
                    }
                }

                // On non Windows platform, we are expecting to have the bytecode stored directly
                if (!Platform.IsWindowsDesktop && bytecode == null)
                {
                    Log.Error("Unable to find compiled shaders [{0}] for mixin [{1}] with parameters [{2}]", compiledUrl, mixin, compilerParameters.ToStringDetailed());
                    throw new InvalidOperationException("Unable to find compiled shaders [{0}]".ToFormat(compiledUrl));
                }

                // ------------------------------------------------------------------------------------------------------------
                // 2) Try to load from intermediate results
                // ------------------------------------------------------------------------------------------------------------
                if (bytecode == null)
                {
                    // Check if this id has already a ShaderBytecodeStore
                    var isObjectInDatabase = database.ObjectDatabase.Exists(ids.CompileParametersId);

                    // Try to load from an existing ShaderBytecode
                    if ((modifiedShaders == null || modifiedShaders.Count == 0) && isObjectInDatabase)
                    {
                        var stream = database.ObjectDatabase.OpenStream(ids.CompileParametersId, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read);
                        using (var resultsStore = new ShaderBytecodeStore(stream))
                        {
                            // Load new values
                            resultsStore.LoadNewValues();

                            var storedValues = resultsStore.GetValues();

                            foreach (KeyValuePair<HashSourceCollection, EffectBytecode> hashResults in storedValues)
                            {
                                if (CheckBytecodeInSyncAgainstSources(hashResults.Value, database))
                                {
                                    bytecode = hashResults.Value;
                                    break;
                                }
                            }
                        }
                    }

                    // --------------------------------------------------------------------------------------------------------
                    // 3) Bytecode was not found in the cache on disk, we need to compile it
                    // --------------------------------------------------------------------------------------------------------
                    if (bytecode == null)
                    {
                        // Open the database for writing
                        var stream = database.ObjectDatabase.OpenStream(ids.CompileParametersId, VirtualFileMode.OpenOrCreate, VirtualFileAccess.ReadWrite, VirtualFileShare.ReadWrite);
                        using (var resultsStore = new ShaderBytecodeStore(stream))
                        {
                            var localLogger = new LoggerResult();

                            // Compile the mixin
                            bytecode = base.Compile(mixin, fullEffectName, compilerParameters, modifiedShaders, recentlyModifiedShaders, localLogger);
                            log.Info("New effect compiled [{0}]\r\n{1}", ids.CompileParametersId, compilerParameters.ToStringDetailed());
                            localLogger.CopyTo(log);

                            // If there are any errors, return immediately
                            if (localLogger.HasErrors)
                            {
                                return null;
                            }

                            // Else store the bytecode for this set of HashSources
                            resultsStore[bytecode.HashSources] = bytecode;
                        }
                    }

                    // Save latest bytecode into the storage
                    using (var stream = database.OpenStream(compiledUrl, VirtualFileMode.Create, VirtualFileAccess.Write, VirtualFileShare.Write))
                    {
                        BinarySerialization.Write(stream, bytecode);
                    }
                }
                else
                {
                    // clone the bytecode since it is not the first time we load it.
                    bytecode = bytecode.Clone();
                }

                // Store the bytecode in the memory cache
                storedResults[ids.FullParametersId] = bytecode;
            }

            return bytecode;
        }

        private void RemoveObsoleteStoredResults(HashSet<string> modifiedShaders)
        {
            lock (storedResults)
            {
                var keysToRemove = new List<ObjectId>();
                foreach (var bytecodePair in storedResults)
                {
                    if (IsBytecodeObsolete(bytecodePair.Value, modifiedShaders))
                        keysToRemove.Add(bytecodePair.Key);
                }

                foreach (var key in keysToRemove)
                    storedResults.Remove(key);
            }
        }

        private bool IsBytecodeObsolete(EffectBytecode bytecode, HashSet<string> modifiedShaders)
        {
            return bytecode.HashSources.Any(x => modifiedShaders.Contains(x.Key));
        }

        /// <summary>
        /// Checks if the specified bytecode is synchronized with latest source
        /// </summary>
        /// <param name="byteCode">The byte code.</param>
        /// <param name="database">The database.</param>
        /// <returns><c>true</c> if bytecode is synchronized with latest source, <c>false</c> otherwise.</returns>
        private static bool CheckBytecodeInSyncAgainstSources(EffectBytecode byteCode, DatabaseFileProvider database)
        {
            var usedSources = byteCode.HashSources;

            // Find a bytecode that is using the same hash for its pdxsl sources
            foreach (var usedSource in usedSources)
            {
                ObjectId currentId;
                if (!database.AssetIndexMap.TryGetValue(usedSource.Key, out currentId) || usedSource.Value != currentId)
                {
                    return false;
                }
            }
            return true;
        }

        private class ShaderBytecodeStore : DictionaryStore<HashSourceCollection, EffectBytecode>
        {
            public ShaderBytecodeStore(Stream stream) : base(stream)
            {
            }
        }
   }
}