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
        private readonly Dictionary<ObjectId, EffectBytecode> bytecodes = new Dictionary<ObjectId, EffectBytecode>();
        private readonly HashSet<ObjectId> bytecodesByPassingStorage = new HashSet<ObjectId>();
        private const string CompiledShadersKey = "__shaders_bytecode__";

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

                // On non Windows platform, we are expecting to have the bytecode stored directly
                if (!Platform.IsWindowsDesktop && bytecode == null)
                {
                    Log.Error("Unable to find compiled shaders [{0}] for mixin [{1}] with parameters [{2}]", compiledUrl, mixin, compilerParameters.ToStringDetailed());
                    throw new InvalidOperationException("Unable to find compiled shaders [{0}]".ToFormat(compiledUrl));
                }

                if (bytecode != null && IsBytecodeObsolete(bytecode))
                {
                    bytecode = null;
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

                ObjectId previousBytecodeId;
                if (!database.AssetIndexMap.TryGetValue(compiledUrl, out previousBytecodeId) || previousBytecodeId != newBytecodeId)
                {
                    localLogger.Info("New effect compiled [{0}] (db: {1})\r\n{1}", mixinObjectId, newBytecodeId, usedParameters.ToStringDetailed());

                    // Using custom serialization to the database to store an object with a custom id
                    var memoryStream = new MemoryStream();
                    BinarySerialization.Write(memoryStream, bytecode);
                    memoryStream.Position = 0;
                    database.ObjectDatabase.Write(memoryStream, newBytecodeId);
                    database.AssetIndexMap[compiledUrl] = newBytecodeId;

                    lock (bytecodes)
                    {
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
                            bytecode = BinarySerialization.Read<EffectBytecode>(stream);
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
 #if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
           foreach (var hashSource in bytecode.HashSources)
            {
                if (GetShaderSourceHash(hashSource.Key) != hashSource.Value)
                {
                    return true;
                }
            }
#endif
            return false;
        }
   }
}