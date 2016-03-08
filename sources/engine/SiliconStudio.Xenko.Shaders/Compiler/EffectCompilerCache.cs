// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    public delegate TaskScheduler TaskSchedulerSelector(ShaderMixinSource mixinTree, EffectCompilerParameters? compilerParameters);

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

        private readonly Dictionary<ObjectId, Task<EffectBytecodeCompilerResult>> compilingShaders = new Dictionary<ObjectId, Task<EffectBytecodeCompilerResult>>();
        private readonly TaskSchedulerSelector taskSchedulerSelector;

        private int effectCompileCount;

        public bool CompileEffectAsynchronously { get; set; }

        public override IVirtualFileProvider FileProvider { get; set; }

        public EffectCompilerCache(EffectCompilerBase compiler, TaskSchedulerSelector taskSchedulerSelector = null) : base(compiler)
        {
            CompileEffectAsynchronously = true;
            this.taskSchedulerSelector = taskSchedulerSelector;
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

        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixin, EffectCompilerParameters? effectCompilerParameters)
        {
            var database = (FileProvider ?? ContentManager.FileProvider) as DatabaseFileProvider;
            if (database == null)
            {
                throw new NotSupportedException("Using the cache requires to ContentManager.FileProvider to be valid.");
            }

            // Forward DatabaseFileProvider to actual compiler here
            // Since we might be in a Task, it has to be forwarded manually (otherwise MicroThreadLocal ones wouldn't work during build)
            // Note: this system might need an overhaul... (too many states?)
            base.FileProvider = database;

            var usedParameters = mixin.UsedParameters;
            var mixinObjectId = ShaderMixinObjectId.Compute(mixin, usedParameters);

            // Final url of the compiled bytecode
            var compiledUrl = string.Format("{0}/{1}", CompiledShadersKey, mixinObjectId);

            EffectBytecode bytecode = null;
            lock (bytecodes)
            {                
                // ------------------------------------------------------------------------------------------------------------
                // 1) Try to load latest bytecode
                // ------------------------------------------------------------------------------------------------------------
                ObjectId bytecodeId;
                if (database.AssetIndexMap.TryGetValue(compiledUrl, out bytecodeId))
                {
                    bytecode = LoadEffectBytecode(database, bytecodeId);
                }

                // On non Windows platform, we are expecting to have the bytecode stored directly
                if (Compiler is NullEffectCompiler && bytecode == null)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("Unable to find compiled shaders [{0}] for mixin [{1}] with parameters [{2}]", compiledUrl, mixin, usedParameters.ToStringPermutationsDetailed());
                    Log.Error(stringBuilder.ToString());
                    throw new InvalidOperationException(stringBuilder.ToString());
                }

                // ------------------------------------------------------------------------------------------------------------
                // 2) Try to load from database cache
                // ------------------------------------------------------------------------------------------------------------
                if (bytecode == null && database.ObjectDatabase.Exists(mixinObjectId))
                {
                    using (var stream = database.ObjectDatabase.OpenStream(mixinObjectId))
                    {
                        // We have an existing stream, make sure the shader is compiled
                        var objectIdBuffer = new byte[ObjectId.HashSize];
                        if (stream.Read(objectIdBuffer, 0, ObjectId.HashSize) == ObjectId.HashSize)
                        {
                            var newBytecodeId = new ObjectId(objectIdBuffer);
                            bytecode = LoadEffectBytecode(database, newBytecodeId);

                            if (bytecode != null)
                            {
                                // If we successfully retrieved it from cache, add it to index map so that it won't be collected and available for faster lookup 
                                database.AssetIndexMap[compiledUrl] = newBytecodeId;
                            }
                        }
                    }
                }
            }

            if (bytecode != null)
            {
                return new EffectBytecodeCompilerResult(bytecode);
            }

            // ------------------------------------------------------------------------------------------------------------
            // 3) Compile the shader
            // ------------------------------------------------------------------------------------------------------------
            lock (compilingShaders)
            {
                Task<EffectBytecodeCompilerResult> compilingShaderTask;
                if (compilingShaders.TryGetValue(mixinObjectId, out compilingShaderTask))
                {
                    // Note: Task might still be compiling
                    return compilingShaderTask;
                }

                // Compile the mixin in a Task
                if (CompileEffectAsynchronously)
                {
                    var resultTask = Task.Factory.StartNew(() => CompileBytecode(mixin, effectCompilerParameters, mixinObjectId, database, compiledUrl, usedParameters), CancellationToken.None, TaskCreationOptions.None, taskSchedulerSelector != null ? taskSchedulerSelector(mixin, effectCompilerParameters) : TaskScheduler.Default);

                    compilingShaders.Add(mixinObjectId, resultTask);

                    return resultTask;
                }
                else
                {
                    return CompileBytecode(mixin, effectCompilerParameters, mixinObjectId, database, compiledUrl, usedParameters);
                }
            }
        }

        private EffectBytecodeCompilerResult CompileBytecode(ShaderMixinSource mixinTree, EffectCompilerParameters? compilerParameters, ObjectId mixinObjectId, DatabaseFileProvider database, string compiledUrl, ShaderMixinParameters usedParameters)
        {
            // Open the database for writing
            var log = new LoggerResult();

            // Note: this compiler is expected to not be async and directly write stuff in localLogger
            var compiledShader = base.Compile(mixinTree, compilerParameters).WaitForResult();
            compiledShader.CompilationLog.CopyTo(log);
            
            // If there are any errors, return immediately
            if (log.HasErrors)
            {
                lock (compilingShaders)
                {
                    compilingShaders.Remove(mixinObjectId);
                }

                return new EffectBytecodeCompilerResult(null, log);
            }

            // Compute the bytecodeId
            var newBytecodeId = compiledShader.Bytecode.ComputeId();

            // Check if we really need to store the bytecode
            lock (bytecodes)
            {
                // Using custom serialization to the database to store an object with a custom id
                // TODO: Check if we really need to write the bytecode everytime even if id is not changed
                var memoryStream = new MemoryStream();
                compiledShader.Bytecode.WriteTo(memoryStream);
                memoryStream.Position = 0;
                database.ObjectDatabase.Write(memoryStream, newBytecodeId, true);
                database.AssetIndexMap[compiledUrl] = newBytecodeId;

                // Save bytecode Id to the database cache as well
                memoryStream.SetLength(0);
                memoryStream.Write((byte[])newBytecodeId, 0, ObjectId.HashSize);
                memoryStream.Position = 0;
                database.ObjectDatabase.Write(memoryStream, mixinObjectId, true);

                if (!bytecodes.ContainsKey(newBytecodeId))
                {
                    log.Verbose("New effect compiled #{0} [{1}] (db: {2})\r\n{3}", effectCompileCount, mixinObjectId, newBytecodeId, usedParameters.ToStringPermutationsDetailed());
                    Interlocked.Increment(ref effectCompileCount);

                    // Replace or add new bytecode
                    bytecodes[newBytecodeId] = compiledShader.Bytecode;
                }
            }

            lock (compilingShaders)
            {
                compilingShaders.Remove(mixinObjectId);
            }

            return compiledShader;
        }

        private EffectBytecode LoadEffectBytecode(DatabaseFileProvider database, ObjectId bytecodeId)
        {
            EffectBytecode bytecode = null;

            if (!bytecodes.TryGetValue(bytecodeId, out bytecode))
            {
                if (!bytecodesByPassingStorage.Contains(bytecodeId) && database.ObjectDatabase.Exists(bytecodeId))
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

            // Always check that the bytecode is in sync with hash sources on all platforms
            if (bytecode != null && IsBytecodeObsolete(bytecode))
            {
                bytecodes.Remove(bytecodeId);
                bytecode = null;
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