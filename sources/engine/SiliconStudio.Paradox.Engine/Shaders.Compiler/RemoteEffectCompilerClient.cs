using System;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Used internally by <see cref="RemoteEffectCompiler"/> to compile shaders remotely,
    /// and <see cref="Rendering.EffectSystem.CreateEffectCompiler"/> to record effect requested.
    /// </summary>
    class RemoteEffectCompilerClient
    {
        private readonly object lockObject = new object();
        private readonly Guid? packageId;
        private Task<SocketMessageLayer> socketMessageLayerTask;

        public RemoteEffectCompilerClient(Guid? packageId)
        {
            this.packageId = packageId;
        }

        public void NotifyEffectUsed(EffectCompileRequest effectCompileRequest)
        {
            Task.Run(async () =>
            {
                // Send any effect request remotely (should fail if not connected)
                var socketMessageLayer = await GetOrCreateConnection();
                await socketMessageLayer.Send(new RemoteEffectCompilerEffectRequested { Request = effectCompileRequest });
            });
        }

        public async Task<SocketMessageLayer> Connect(Guid? packageId)
        {
            var url = string.Format("/service/{0}/SiliconStudio.Paradox.EffectCompilerServer.exe", ParadoxVersion.CurrentAsText);
            if (packageId.HasValue)
                url += string.Format("?packageid={0}", packageId.Value);

            var socketContext = await RouterClient.RequestServer(url);

            var socketMessageLayer = new SocketMessageLayer(socketContext, false);

            // Register network VFS
            NetworkVirtualFileProvider.RegisterServer(socketMessageLayer);

            Task.Run(() => socketMessageLayer.MessageLoop());

            return socketMessageLayer;
        }

        public async Task<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            // Make sure we are connected
            // TODO: Handle reconnections, etc...
            var socketMessageLayer = await GetOrCreateConnection();

            var shaderCompilerAnswer = (RemoteEffectCompilerEffectAnswer)await socketMessageLayer.SendReceiveAsync(new RemoteEffectCompilerEffectRequest
            {
                MixinTree = mixinTree,
                UsedParameters = mixinTree.UsedParameters,
            });

            // TODO: Get LoggerResult as well
            return new EffectBytecodeCompilerResult(shaderCompilerAnswer.EffectBytecode);
        }

        private async Task<SocketMessageLayer> GetOrCreateConnection()
        {
            // Lazily connect
            lock (lockObject)
            {
                if (socketMessageLayerTask == null)
                    socketMessageLayerTask = Task.Run(() => Connect(packageId));
            }

            return await socketMessageLayerTask;
        }
    }
}