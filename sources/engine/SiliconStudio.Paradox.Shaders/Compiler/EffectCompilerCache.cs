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
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    public struct TaskOrResult<T> where T : class
    {
        public readonly T Result;
        public readonly Task<T> Task;

        public static implicit operator TaskOrResult<T>(T result)
        {
            return new TaskOrResult<T>(result);
        }

        public static implicit operator TaskOrResult<T>(Task<T> task)
        {
            return new TaskOrResult<T>(task);
        }

        public TaskOrResult(Task<T> task)
        {
            Result = default(T);
            Task = task;
        }

        public TaskOrResult(T result)
        {
            Result = result;
            Task = null;
        }

        public T WaitForResult()
        {
            if (Task != null)
                return Task.Result;

            return Result;
        }
    }

    /// <summary>
    /// Checks if an effect has already been compiled in its cache before deferring to a real <see cref="IEffectCompiler"/>.
    /// </summary>
    [DataSerializerGlobal(null, typeof(KeyValuePair<HashSourceCollection, EffectBytecode>))]
    public class EffectCompilerCache : EffectCompilerChain
    {
        private readonly TaskScheduler taskScheduler;

        private static readonly Logger Log = GlobalLogger.GetLogger("EffectCompilerCache");
        private readonly Dictionary<ObjectId, EffectBytecode> bytecodes = new Dictionary<ObjectId, EffectBytecode>();
        private readonly HashSet<ObjectId> bytecodesByPassingStorage = new HashSet<ObjectId>();
        private const string CompiledShadersKey = "__shaders_bytecode__";

        private readonly Dictionary<ObjectId, Task<EffectBytecode>> compilingShaders = new Dictionary<ObjectId, Task<EffectBytecode>>();

        private int effectCompileCount;

        public bool CompileEffectAsynchronously { get; set; }

        public override DatabaseFileProvider FileProvider { get; set; }

        public EffectCompilerCache(EffectCompilerBase compiler, TaskScheduler taskScheduler = null) : base(compiler)
        {
            CompileEffectAsynchronously = true;
            this.taskScheduler = taskScheduler ?? TaskScheduler.Default;
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

        public override TaskOrResult<EffectBytecode> Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log)
        {
            var database = FileProvider ?? AssetManager.FileProvider;
            if (database == null)
            {
                throw new NotSupportedException("Using the cache requires to AssetManager.FileProvider to be valid.");
            }

            // Forward DatabaseFileProvider to actual compiler here
            // Since we might be in a Task, it has to be forwarded manually (otherwise MicroThreadLocal ones wouldn't work during build)
            // Note: this system might need an overhaul... (too many states?)
            base.FileProvider = database;

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
                ObjectId bytecodeId;
                if (database.AssetIndexMap.TryGetValue(compiledUrl, out bytecodeId))
                {
                    bytecode = LoadEffectBytecode(database, bytecodeId);
                }

                // On non Windows platform, we are expecting to have the bytecode stored directly
                if (!Platform.IsWindowsDesktop && bytecode == null)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("Unable to find compiled shaders [{0}] for mixin [{1}] with parameters [{2}]", compiledUrl, mixin, usedParameters.ToStringDetailed());
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
                return bytecode;
            }

            // ------------------------------------------------------------------------------------------------------------
            // 3) Compile the shader
            // ------------------------------------------------------------------------------------------------------------
            lock (compilingShaders)
            {
                Task<EffectBytecode> compilingShaderTask;
                if (compilingShaders.TryGetValue(mixinObjectId, out compilingShaderTask))
                {
                    Debugger.Log(0, "EffectCompiler", string.Format("Using pending compile of {0}...\n", mixinObjectId));

                    // Note: Task might still be compiling
                    return compilingShaderTask;
                }

                // Open the database for writing
                var localLogger = new LoggerResult();

                Debugger.Log(0, "EffectCompiler", string.Format("Starting compile of {0}...\n", mixinObjectId));

                // Compile the mixin in a Task
                if (CompileEffectAsynchronously)
                {
                    var resultTask = Task.Factory.StartNew(() => CompileBytecode(mixinTree, compilerParameters, log, localLogger, mixinObjectId, database, compiledUrl, usedParameters), CancellationToken.None, TaskCreationOptions.None, taskScheduler);

                    compilingShaders.Add(mixinObjectId, resultTask);

                    return resultTask;
                }
                else
                {
                    return CompileBytecode(mixinTree, compilerParameters, log, localLogger, mixinObjectId, database, compiledUrl, usedParameters);
                }
            }
        }

        private EffectBytecode CompileBytecode(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log, LoggerResult localLogger, ObjectId mixinObjectId, DatabaseFileProvider database, string compiledUrl, ShaderMixinParameters usedParameters)
        {
            var compiledShader = base.Compile(mixinTree, compilerParameters, localLogger);
            var bytecode2 = compiledShader.Result;
            localLogger.CopyTo(log);

            // If there are any errors, return immediately
            if (localLogger.HasErrors)
            {
                lock (compilingShaders)
                {
                    compilingShaders.Remove(mixinObjectId);
                }

                return null;
            }

            // Compute the bytecodeId
            var newBytecodeId = bytecode2.ComputeId();

            // Check if we really need to store the bytecode
            lock (bytecodes)
            {
                // Using custom serialization to the database to store an object with a custom id
                // TODO: Check if we really need to write the bytecode everytime even if id is not changed
                var memoryStream = new MemoryStream();
                bytecode2.WriteTo(memoryStream);
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
                    log.Info("New effect compiled #{0} [{1}] (db: {2})\r\n{3}", effectCompileCount, mixinObjectId, newBytecodeId, usedParameters.ToStringDetailed());
                    Interlocked.Increment(ref effectCompileCount);

                    // Replace or add new bytecode
                    bytecodes[newBytecodeId] = bytecode2;
                }
            }

            lock (compilingShaders)
            {
                compilingShaders.Remove(mixinObjectId);
            }

            Debugger.Log(0, "EffectCompiler", string.Format("Finishing compile of {0}...\n", mixinObjectId));

            return bytecode2;
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