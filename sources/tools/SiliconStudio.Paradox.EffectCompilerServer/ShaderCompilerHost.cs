using System;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Shaders.Compiler;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    /// <summary>
    /// Shader compiler host (over network)
    /// </summary>
    public class ShaderCompilerHost
    {
        public void Listen(int port)
        {
            // TODO: Asynchronously initialize Irony grammars to improve first compilation request performance?

            var socketContext = CreateSocketContext();
            socketContext.StartServer(port, false);
        }

        /// <summary>
        /// Tries to connect. Blocks until connection fails or happens (if connection happens, it will launch the message loop in a separate unobserved Task).
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public async Task TryConnect(string address, int port)
        {
            var socketContext = CreateSocketContext();

            // Wait for a connection to be possible on adb forwarded port
            await socketContext.StartClient(address, port);
        }

        private SocketContext CreateSocketContext()
        {
            var socketContext = new SocketContext();
            socketContext.Connected = (clientSocketContext) =>
            {
                // Create an effect compiler per connection
                var effectCompiler = new EffectCompiler();

                Console.WriteLine("Client connected");

                var tempFilename = Path.GetTempFileName();
                var fileStream = new FileStream(tempFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                // TODO: Properly close the file, and choose where to copy/move it?
                var recordedEffectCompile = new EffectLogStore(fileStream);

                // TODO: This should come from an "init" packet
                effectCompiler.SourceDirectories.Add(EffectCompilerBase.DefaultSourceShaderFolder);

                // Make a VFS that will access remotely the DatabaseFileProvider
                // TODO: Is that how we really want to do that in the future?
                var networkVFS = new NetworkVirtualFileProvider(clientSocketContext, "/asset");
                VirtualFileSystem.RegisterProvider(networkVFS);
                effectCompiler.FileProvider = networkVFS;

                clientSocketContext.AddPacketHandler<ShaderCompilerRequest>((packet) => ShaderCompilerRequestHandler(clientSocketContext, recordedEffectCompile, effectCompiler, packet));

                Task.Run(() => clientSocketContext.MessageLoop());
            };
            socketContext.Disconnected = (clientSocketContext) =>
            {
                Console.WriteLine("Client disconnected");
            };

            return socketContext;
        }

        private async void ShaderCompilerRequestHandler(SocketContext clientSocketContext, EffectLogStore recordedEffectCompile, EffectCompiler effectCompiler, ShaderCompilerRequest shaderCompilerRequest)
        {
            // Yield so that this socket can continue its message loop to answer to shader file request.
            await Task.Yield();

            Console.WriteLine("Compiling shader");

            // Restore MixinTree.UsedParameters (since it is DataMemberIgnore)
            shaderCompilerRequest.MixinTree.UsedParameters = shaderCompilerRequest.UsedParameters;

            // A shader has been requested, compile it (asynchronously)!
            var precompiledEffectShaderPass = await effectCompiler.Compile(shaderCompilerRequest.MixinTree, null).AwaitResult();

            // Record compilation to asset file (only if parent)
            recordedEffectCompile[new EffectCompileRequest(shaderCompilerRequest.MixinTree.Name, shaderCompilerRequest.MixinTree.UsedParameters)] = true;
            
            // Send compiled shader
            clientSocketContext.Send(new ShaderCompilerAnswer { StreamId = shaderCompilerRequest.StreamId, EffectBytecode = precompiledEffectShaderPass.Bytecode });
        }
    }
}