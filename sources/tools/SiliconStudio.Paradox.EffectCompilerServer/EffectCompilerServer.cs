using System;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.ConnectionRouter;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Shaders.Compiler;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    /// <summary>
    /// Shader compiler host (over network)
    /// </summary>
    public class EffectCompilerServer : RouterServiceServer
    {
        public EffectCompilerServer() : base(string.Format("/{0}/SiliconStudio.Paradox.EffectCompilerServer.exe", ParadoxVersion.CurrentAsText))
        {
            // TODO: Asynchronously initialize Irony grammars to improve first compilation request performance?a
        }

        protected override void RunServer(SimpleSocket clientSocket)
        {
            // Create an effect compiler per connection
            var effectCompiler = new EffectCompiler();

            var socketMessageLoop = new SocketMessageLayer(clientSocket, true);

            Console.WriteLine("Client connected");

            var tempFilename = Path.GetTempFileName();
            var fileStream = new FileStream(tempFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            // TODO: Properly close the file, and choose where to copy/move it?
            var recordedEffectCompile = new EffectLogStore(fileStream);

            // TODO: This should come from an "init" packet
            effectCompiler.SourceDirectories.Add(EffectCompilerBase.DefaultSourceShaderFolder);

            // Make a VFS that will access remotely the DatabaseFileProvider
            // TODO: Is that how we really want to do that in the future?
            var networkVFS = new NetworkVirtualFileProvider(socketMessageLoop, "/asset");
            VirtualFileSystem.RegisterProvider(networkVFS);
            effectCompiler.FileProvider = networkVFS;

            socketMessageLoop.AddPacketHandler<ShaderCompilerRequest>((packet) => ShaderCompilerRequestHandler(socketMessageLoop, recordedEffectCompile, effectCompiler, packet));

            Task.Run(() => socketMessageLoop.MessageLoop());
        }

        private static async Task ShaderCompilerRequestHandler(SocketMessageLayer socketMessageLayer, EffectLogStore recordedEffectCompile, EffectCompiler effectCompiler, ShaderCompilerRequest shaderCompilerRequest)
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
            socketMessageLayer.Send(new ShaderCompilerAnswer { StreamId = shaderCompilerRequest.StreamId, EffectBytecode = precompiledEffectShaderPass.Bytecode });
        }
    }
}