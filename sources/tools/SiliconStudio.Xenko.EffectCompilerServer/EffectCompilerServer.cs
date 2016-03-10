// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.ConnectionRouter;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Shaders.Compiler;
using SiliconStudio.Xenko.Shaders.Compiler.Internals;

namespace SiliconStudio.Xenko.EffectCompilerServer
{
    /// <summary>
    /// Shader compiler host (over network)
    /// </summary>
    public class EffectCompilerServer : RouterServiceServer
    {
        private Dictionary<Guid, SocketMessageLayer> gameStudioPerPackageId = new Dictionary<Guid, SocketMessageLayer>();

        public EffectCompilerServer() : base(string.Format("/service/{0}/SiliconStudio.Xenko.EffectCompilerServer.exe", XenkoVersion.CurrentAsText))
        {
            // TODO: Asynchronously initialize Irony grammars to improve first compilation request performance?a
        }

        /// <inheritdoc/>
        protected override async void HandleClient(SimpleSocket clientSocket, string url)
        {
            string[] urlSegments;
            string urlParameters;
            RouterHelper.ParseUrl(url, out urlSegments, out urlParameters);
            var parameters = RouterHelper.ParseQueryString(urlParameters);
            var mode = parameters["mode"];

            // We accept everything
            await AcceptConnection(clientSocket);

            var socketMessageLayer = new SocketMessageLayer(clientSocket, true);

            Guid? packageId = null;
            {
                Guid packageIdParsed;
                if (Guid.TryParse(parameters["packageid"], out packageIdParsed))
                    packageId = packageIdParsed;
            }

            if (mode == "gamestudio")
            {
                Console.WriteLine("GameStudio mode started!");

                if (!packageId.HasValue)
                    return;

                lock (gameStudioPerPackageId)
                {
                    gameStudioPerPackageId[packageId.Value] = socketMessageLayer;
                }
            }
            else
            {
                // Create an effect compiler per connection
                var effectCompiler = new EffectCompiler();

                Console.WriteLine("Client connected");

                // TODO: Properly close the file, and choose where to copy/move it?
                var recordedEffectCompile = new EffectLogStore(new MemoryStream());

                // TODO: This should come from an "init" packet
                effectCompiler.SourceDirectories.Add(EffectCompilerBase.DefaultSourceShaderFolder);

                // Make a VFS that will access remotely the DatabaseFileProvider
                // TODO: Is that how we really want to do that in the future?
                var networkVFS = new NetworkVirtualFileProvider(socketMessageLayer, "/asset");
                VirtualFileSystem.RegisterProvider(networkVFS);
                effectCompiler.FileProvider = networkVFS;

                socketMessageLayer.AddPacketHandler<RemoteEffectCompilerEffectRequest>((packet) => ShaderCompilerRequestHandler(socketMessageLayer, recordedEffectCompile, effectCompiler, packet));

                socketMessageLayer.AddPacketHandler<RemoteEffectCompilerEffectRequested>((packet) =>
                {
                    if (!packageId.HasValue)
                        return;

                    SocketMessageLayer gameStudio;
                    lock (gameStudioPerPackageId)
                    {
                        if (!gameStudioPerPackageId.TryGetValue(packageId.Value, out gameStudio))
                            return;
                    }

                    // Forward to game studio
                    gameStudio.Send(packet);
                });
            }

            Task.Run(() => socketMessageLayer.MessageLoop());
        }

        private static async Task ShaderCompilerRequestHandler(SocketMessageLayer socketMessageLayer, EffectLogStore recordedEffectCompile, EffectCompiler effectCompiler, RemoteEffectCompilerEffectRequest remoteEffectCompilerEffectRequest)
        {
            // Yield so that this socket can continue its message loop to answer to shader file request
            // TODO: maybe not necessary anymore with RouterServiceServer?
            await Task.Yield();

            Console.WriteLine("Compiling shader");

            // A shader has been requested, compile it (asynchronously)!
            var precompiledEffectShaderPass = await effectCompiler.Compile(remoteEffectCompilerEffectRequest.MixinTree, null).AwaitResult();

            // Record compilation to asset file (only if parent)
            recordedEffectCompile[new EffectCompileRequest(remoteEffectCompilerEffectRequest.MixinTree.Name, remoteEffectCompilerEffectRequest.CompilerParameters)] = true;
            
            // Send compiled shader
            socketMessageLayer.Send(new RemoteEffectCompilerEffectAnswer { StreamId = remoteEffectCompilerEffectRequest.StreamId, EffectBytecode = precompiledEffectShaderPass.Bytecode });
        }
    }
}