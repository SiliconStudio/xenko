// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    class EffectCompilerRemote : EffectCompilerBase
    {
        private static ShaderCompilerTarget shaderCompilerTarget;
        private static Task shaderCompilerConnected;

        public override IVirtualFileProvider FileProvider
        {
            get { return null; }
            set {}
        }

        public static void Connect()
        {
            // TODO: Delay connection until actually needed
            // TODO: Display a log message
            // TODO: Try both to connect to server and client at the same time?
            if (shaderCompilerTarget == null)
            {
                shaderCompilerTarget = new ShaderCompilerTarget();
                shaderCompilerConnected = shaderCompilerTarget.Connect();
            }
        }

        public override ObjectId GetShaderSourceHash(string type)
        {
            var url = GetStoragePathFromShaderType(type);
            ObjectId shaderSourceId;
            AssetManager.FileProvider.AssetIndexMap.TryGetValue(url, out shaderSourceId);
            return shaderSourceId;
        }

        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            return CompileAsync(mixinTree, compilerParameters);
        }

        private async Task<EffectBytecodeCompilerResult> CompileAsync(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            // Make sure we are connected
            // TODO: Handle reconnections, etc...
            await shaderCompilerConnected;

            return await shaderCompilerTarget.Compile(mixinTree, compilerParameters);
        }
    }

    public class ShaderCompilerInitializeRequest : SocketMessage
    {
        public Dictionary<ObjectId, byte[]> ShaderSources { get; set; }
    }

    public class ShaderCompilerRequest : SocketMessage
    {
        public ShaderMixinSource MixinTree { get; set; }
        
        // MixinTree.UsedParameters is DataMemberIgnore, so transmit it manually
        public ShaderMixinParameters UsedParameters { get; set; }
    }

    public class ShaderCompilerAnswer : SocketMessage
    {
        // TODO: Support LoggerResult as well
        public EffectBytecode EffectBytecode { get; set; }
    }

    class ShaderCompilerTarget
    {
        private TaskCompletionSource<SocketMessageLayer> socketMessageLoopTCS = new TaskCompletionSource<SocketMessageLayer>();
        private bool initialized = false;

        public async Task Connect()
        {
            var socketContext = await RouterClient.RequestServer(string.Format("/{0}/SiliconStudio.Paradox.EffectCompilerServer.exe", ParadoxVersion.CurrentAsText));

            var socketMessageLoop = new SocketMessageLayer(socketContext, false);

            // Register network VFS
            NetworkVirtualFileProvider.RegisterServer(socketMessageLoop);

            socketMessageLoopTCS.TrySetResult(socketMessageLoop);

            Task.Run(() => socketMessageLoop.MessageLoop());
        }

        public async Task<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            var socketMessageLoop = await socketMessageLoopTCS.Task;

            var shaderCompilerAnswer = (ShaderCompilerAnswer)await socketMessageLoop.SendReceiveAsync(new ShaderCompilerRequest
            {
                MixinTree = mixinTree,
                UsedParameters = mixinTree.UsedParameters,
            });

            // TODO: Get LoggerResult as well
            return new EffectBytecodeCompilerResult(shaderCompilerAnswer.EffectBytecode);
        }
    }
}