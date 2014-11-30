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

            var ids = ShaderMixinObjectId.Compute(mixin, usedParameters);

            EffectBytecode bytecode = null;
            lock (bytecodes)
            {                
                // Final url of the compiled bytecode
                var compiledUrl = string.Format("{0}/{1}", CompiledShadersKey, ids.CompileParametersId);

                // ------------------------------------------------------------------------------------------------------------
                // 1) Try to load latest bytecode
                // ------------------------------------------------------------------------------------------------------------
                bytecode = LoadEffectBytecode(compiledUrl, false);

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

                // ------------------------------------------------------------------------------------------------------------
                // 2) Try to load from intermediate results
                // ------------------------------------------------------------------------------------------------------------
                if (bytecode == null)
                {
                    // Open the database for writing
                    var localLogger = new LoggerResult();

                    // Compile the mixin
                    bytecode = base.Compile(mixinTree, compilerParameters, localLogger);
                    localLogger.Info("New effect compiled [{0}]\r\n{1}", ids.CompileParametersId, usedParameters.ToStringDetailed());
                    localLogger.CopyTo(log);

                    // If there are any errors, return immediately
                    if (localLogger.HasErrors)
                    {
                        return null;
                    }

                    // Save latest bytecode into the storage
                    using (var stream = database.OpenStream(compiledUrl, VirtualFileMode.Create, VirtualFileAccess.Write, VirtualFileShare.Write))
                    {
                        BinarySerialization.Write(stream, bytecode);
                    }

                    bytecode = LoadEffectBytecode(compiledUrl, true);
                }
            }

            return bytecode;
        }

        private EffectBytecode LoadEffectBytecode(string url, bool alwaysUseStorage)
        {
            var database = AssetManager.FileProvider;
            ObjectId bytecodeId;
            EffectBytecode bytecode = null;
            if (database.AssetIndexMap.TryGetValue(url, out bytecodeId))
            {
                if (!bytecodes.TryGetValue(bytecodeId, out bytecode))
                {
                    if (alwaysUseStorage || !bytecodesByPassingStorage.Contains(bytecodeId))
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