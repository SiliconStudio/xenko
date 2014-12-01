// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
        private readonly Dictionary<ObjectId, EffectBytecode> bytecodes = new Dictionary<ObjectId, EffectBytecode>();
        private readonly HashSet<ObjectId> bytecodesByPassingStorage = new HashSet<ObjectId>();
        private const string CompiledShadersKey = "__shaders_bytecode__";

        private int effectCompileCount;

        public EffectCompilerCache(EffectCompilerBase compiler) : base(compiler)
        {
        }

        public override void ResetCache(HashSet<string> modifiedShaders)
        {
            // remove old shaders from cache
            lock (bytecodes)
            {
                base.ResetCache(modifiedShaders);
                RemoveObsoleteStoredResults(modifiedShaders);
            }
        }

        public override EffectBytecode Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log)
        {
            var database = AssetManager.FileProvider;
            if (database == null)
            {
                throw new NotSupportedException("Using the cache requires to AssetManager.FileProvider to be valid.");
            }

            var mixin = mixinTree.Mixin;
            var usedParameters = mixinTree.UsedParameters;

            var mixinObjectId = ShaderMixinObjectId.Compute(mixin, usedParameters);

            // Final url of the compiled bytecode
            var compiledUrl = string.Format("{0}/{1}", CompiledShadersKey, mixinObjectId);

            EffectBytecode bytecode = null;
            lock (bytecodes)
            {                
                // ------------------------------------------------------------------------------------------------------------
                // 1) Try to load latest bytecode
                // ------------------------------------------------------------------------------------------------------------
                bytecode = LoadEffectBytecode(compiledUrl);

                // Always check that the bytecode is in sync with hash sources on all platforms
                if (bytecode != null && IsBytecodeObsolete(bytecode))
                {
                    bytecode = null;
                }

                // On non Windows platform, we are expecting to have the bytecode stored directly
                if (!Platform.IsWindowsDesktop && bytecode == null)
                {
                    Log.Error("Unable to find compiled shaders [{0}] for mixin [{1}] with parameters [{2}]", compiledUrl, mixin, compilerParameters.ToStringDetailed());
                    throw new InvalidOperationException("Unable to find compiled shaders [{0}]".ToFormat(compiledUrl));
                }
            }

            // ------------------------------------------------------------------------------------------------------------
            // 2) Try to load from intermediate results
            // ------------------------------------------------------------------------------------------------------------
            if (bytecode == null)
            {
                // Open the database for writing
                var localLogger = new LoggerResult();

                // Compile the mixin
                bytecode = base.Compile(mixinTree, compilerParameters, localLogger);
                localLogger.CopyTo(log);

                // If there are any errors, return immediately
                if (localLogger.HasErrors)
                {
                    return null;
                }

                // Because ShaderBytecode.Data can vary, we are calculating the bytecodeId without it (but with the ShaderBytecode.Id)
                var previousStages = new byte[bytecode.Stages.Length][];
                for (int i = 0; i < bytecode.Stages.Length; i++)
                {
                    previousStages[i] = bytecode.Stages[i].Data;
                    bytecode.Stages[i].Data = null;
                }

                // Not optimized: Pre-calculate bytecodeId in order to avoid writing to same storage
                ObjectId newBytecodeId;
                var memStream = new MemoryStream();
                using (var stream = new DigestStream(memStream))
                {
                    BinarySerialization.Write(stream, bytecode);
                    newBytecodeId = stream.CurrentHash;
                }

                // Revert back
                for (int i = 0; i < bytecode.Stages.Length; i++)
                {
                    bytecode.Stages[i].Data = previousStages[i];
                }

                // Check if we really need to store the bytecode
                lock (bytecodes)
                {
                    // Using custom serialization to the database to store an object with a custom id
                    // TODO: Check if we really need to write the bytecode everytime even if id is not changed
                    var memoryStream = new MemoryStream();
                    bytecode.WriteTo(memoryStream);
                    memoryStream.Position = 0;
                    database.ObjectDatabase.Write(memoryStream, newBytecodeId);
                    database.AssetIndexMap[compiledUrl] = newBytecodeId;

                    if (!bytecodes.ContainsKey(newBytecodeId))
                    {
                        log.Info("New effect compiled #{0} [{1}] (db: {2})\r\n{3}", effectCompileCount, mixinObjectId, newBytecodeId, usedParameters.ToStringDetailed());
                        Interlocked.Increment(ref effectCompileCount);

                        // Replace or add new bytecode
                        bytecodes[newBytecodeId] = bytecode;
                    }
                }
            }

            return bytecode;
        }

        private EffectBytecode LoadEffectBytecode(string url)
        {
            var database = AssetManager.FileProvider;
            ObjectId bytecodeId;
            EffectBytecode bytecode = null;
            if (database.AssetIndexMap.TryGetValue(url, out bytecodeId))
            {
                if (!bytecodes.TryGetValue(bytecodeId, out bytecode))
                {
                    if (!bytecodesByPassingStorage.Contains(bytecodeId))
                    {
                        using (var stream = database.ObjectDatabase.OpenStream(bytecodeId))
                        {
                            bytecode = EffectBytecode.FromStream(stream);
                        }
                    }
                    if (bytecode != null)
                    {
                        bytecodes.Add(bytecodeId, bytecode);
                    }
                }
            }
            return bytecode;
        }

        private void RemoveObsoleteStoredResults(HashSet<string> modifiedShaders)
        {
            // TODO: avoid List<ObjectId> creation?
            var keysToRemove = new List<ObjectId>();
            foreach (var bytecodePair in bytecodes)
            {
                if (IsBytecodeObsolete(bytecodePair.Value, modifiedShaders))
                    keysToRemove.Add(bytecodePair.Key);
            }

            foreach (var key in keysToRemove)
            {
                bytecodes.Remove(key);
                bytecodesByPassingStorage.Add(key);
            }
        }

        private bool IsBytecodeObsolete(EffectBytecode bytecode, HashSet<string> modifiedShaders)
        {
            // Don't use linq
            foreach (KeyValuePair<string, ObjectId> x in bytecode.HashSources)
            {
                if (modifiedShaders.Contains(x.Key)) return true;
            }
            return false;
        }

        private bool IsBytecodeObsolete(EffectBytecode bytecode)
        {
           foreach (var hashSource in bytecode.HashSources)
            {
                if (GetShaderSourceHash(hashSource.Key) != hashSource.Value)
                {
                    return true;
                }
            }
            return false;
        }
   }
}