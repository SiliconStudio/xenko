// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System.Collections.Generic;
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

        public EffectCompilerRemote()
        {
            // TODO: Handle multiple effect compiler remote?
            // TODO: Hardcoded for android forward case
            if (shaderCompilerTarget == null)
            {
                shaderCompilerTarget = new ShaderCompilerTarget();
                shaderCompilerConnected = shaderCompilerTarget.Connect(1244);
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
    }

    public class ShaderCompilerAnswer : SocketMessage
    {
        // TODO: Support LoggerResult as well
        public EffectBytecode EffectBytecode { get; set; }
    }

    public class ShaderCompilerTarget
    {
        private SocketContext socketContext = new SocketContext();
        private TaskCompletionSource<SocketContext> socketContextClientTCS = new TaskCompletionSource<SocketContext>();

        public Task Connect(int port)
        {
            socketContext.Connected = context =>
            {
                // Register network VFS
                NetworkVirtualFileProvider.RegisterServer(context);

                socketContextClientTCS.TrySetResult(context);
            };

            socketContext.StartServer(port);

            // Wait for server to connect to us (as a Task)
            return socketContextClientTCS.Task;
        }

        public async Task<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            var socketContextClient = await socketContextClientTCS.Task;

            var shaderCompilerAnswer = (ShaderCompilerAnswer)await socketContextClient.SendReceiveAsync(new ShaderCompilerRequest
            {
                MixinTree = mixinTree,
            });

            // TODO: Get LoggerResult as well
            return new EffectBytecodeCompilerResult(shaderCompilerAnswer.EffectBytecode);
        }
    }
}
#endif
