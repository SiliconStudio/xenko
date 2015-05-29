// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Shaders.Compiler;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup adb port forward
            // TODO: Currently hardcoded
            ShellHelper.RunProcessAndGetOutput(@"adb", @"forward tcp:1244 tcp:1244");

            var shaderCompilerServer = new ShaderCompilerHost();

            // Wait and process messages
            // TODO: Rearrange how thread/async is done
            // TODO: We should support both client and server mode for socket connection
            while (true)
            {
                shaderCompilerServer.TryConnect(1244).Wait();
            }
        }
    }

    /// <summary>
    /// Shader compiler host (over network)
    /// </summary>
    public class ShaderCompilerHost
    {
        private SocketContext socketContext;
        private TaskCompletionSource<bool> clientConnectedTCS;

        public Task TryConnect(int port)
        {
            socketContext = new SocketContext();
            clientConnectedTCS = new TaskCompletionSource<bool>();

            socketContext.Connected = (clientSocketContext) =>
            {
                // Create an effect compiler per connection
                var effectCompiler = new EffectCompiler();

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

                clientConnectedTCS.TrySetResult(true);
            };

            // Wait for a connection to be possible on adb forwarded port
            var clientDone = socketContext.StartClient(IPAddress.Loopback, port);

            return clientDone;
        }

        private async void ShaderCompilerRequestHandler(SocketContext clientSocketContext, EffectLogStore recordedEffectCompile, EffectCompiler effectCompiler, ShaderCompilerRequest shaderCompilerRequest)
        {
            // Wait for a client to be connected
            await clientConnectedTCS.Task;

            // Yield so that this socket can continue its message loop to answer to shader file request.
            await Task.Yield();

            Console.WriteLine("Compiling shader");

            // A shader has been requested, compile it (asynchronously)!
            var precompiledEffectShaderPass = await effectCompiler.Compile(shaderCompilerRequest.MixinTree, null).AwaitResult();

            // Record compilation to asset file (only if parent)
            recordedEffectCompile[new EffectCompileRequest(shaderCompilerRequest.MixinTree.Name, shaderCompilerRequest.MixinTree.UsedParameters)] = true;
            
            // Send compiled shader
            clientSocketContext.Send(new ShaderCompilerAnswer { StreamId = shaderCompilerRequest.StreamId, EffectBytecode = precompiledEffectShaderPass.Bytecode });
        }
    }
}