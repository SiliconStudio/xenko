// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    public class Router
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("Router");

        private Dictionary<string, TaskCompletionSource<SocketContext>> registeredServices = new Dictionary<string, TaskCompletionSource<SocketContext>>();
        private Dictionary<Guid, TaskCompletionSource<SocketContext>> pendingServers = new Dictionary<Guid, TaskCompletionSource<SocketContext>>();

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
            socketContext.Connected = async (clientSocketContext) =>
            {
                try
                {
                    // Routing
                    var routerMessage = (RouterMessage)await clientSocketContext.ReadStream.Read7BitEncodedInt();

                    Log.Info("Client {0}:{1} connected, with message {2}", clientSocketContext.RemoteAddress, clientSocketContext.RemotePort, routerMessage);

                    switch (routerMessage)
                    {
                        case RouterMessage.ServiceProvideServer:
                        {
                            await HandleMessageServiceProvideServer(clientSocketContext);
                            break;
                        }
                        case RouterMessage.ServerStarted:
                        {
                            await HandleMessageServerStarted(clientSocketContext);
                            break;
                        }
                        case RouterMessage.ClientRequestServer:
                        {
                            await HandleMessageClientRequestServer(clientSocketContext);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException(string.Format("Router: Unknown message: {0}", routerMessage));
                    }
                }
                catch (Exception e)
                {
                    // TODO: Ideally, separate socket-related error messages (disconnection) from real errors
                    // Unfortunately, it seems WinRT returns Exception, so it seems we can't filter with SocketException/IOException only?
                    Log.Info("Client {0}:{1} disconnected with exception: {2}", clientSocketContext.RemoteAddress, clientSocketContext.RemotePort, e.Message);
                    clientSocketContext.Dispose();
                }
            };

            return socketContext;
        }

        /// <summary>
        /// Handles ClientRequestServer messages.
        /// It will try to find a matching service (spawn it if not started yet), and ask it to establish a new "server" connection back to us.
        /// </summary>
        /// <param name="clientSocketContext">The client socket context.</param>
        /// <returns></returns>
        private async Task HandleMessageClientRequestServer(SocketContext clientSocketContext)
        {
            // Check for an existing server
            // TODO: Proper Url parsing (query string)
            var url = await clientSocketContext.ReadStream.ReadStringAsync();

            // Find a matching server
            TaskCompletionSource<SocketContext> serviceTCS;

            lock (registeredServices)
            {
                if (!registeredServices.TryGetValue(url, out serviceTCS))
                {
                    serviceTCS = new TaskCompletionSource<SocketContext>();
                    registeredServices.Add(url, serviceTCS);
                }

                if (!serviceTCS.Task.IsCompleted)
                {
                    var urlSegments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (urlSegments.Length != 2)
                    {
                        Log.Error("{0} action URL {1} is invalid", RouterMessage.ClientRequestServer, url);
                        clientSocketContext.Dispose();
                        return;
                    }

                    var paradoxVersion = urlSegments[0].Trim('/');
                    var serviceExe = urlSegments[1];

                    var paradoxSdkDir = RouterHelper.FindParadoxSdkDir(paradoxVersion);
                    if (paradoxSdkDir == null)
                    {
                        Log.Error("{0} action URL {1} references uninstalled Paradox", RouterMessage.ClientRequestServer, url);
                        clientSocketContext.Dispose();
                        return;
                    }

                    var servicePath = Path.Combine(paradoxSdkDir, @"Bin\Windows-Direct3D11", serviceExe);
                    var process = Process.Start(servicePath);

                    // Let's tie lifetime of spawned process to ours
                    // TODO: Move that in a better namespace
                    new GameStudio.Plugin.Debugging.AttachedChildProcessJob(process);
                }
            }

            var service = await serviceTCS.Task;

            // Generate connection Guid
            var guid = Guid.NewGuid();
            var serverSocketTCS = new TaskCompletionSource<SocketContext>();
            lock (pendingServers)
            {
                pendingServers.Add(guid, serverSocketTCS);
            }

            // Notify service that we want it to establish back a new connection to us for this client
            await service.WriteStream.Write7BitEncodedInt((int)RouterMessage.ServiceRequestServer);
            await service.WriteStream.WriteGuidAsync(guid);
            await service.WriteStream.FlushAsync();

            // Wait for such a server to be available
            var serverSocketContext = await serverSocketTCS.Task;

            try
            {
                // Notify client that we've found a server for it
                await clientSocketContext.WriteStream.Write7BitEncodedInt((int)RouterMessage.ClientServerStarted);
                await clientSocketContext.WriteStream.FlushAsync();

                // Let's forward clientSocketContext and serverSocketContext
                await await Task.WhenAny(
                    ForwardSocket(clientSocketContext, serverSocketContext),
                    ForwardSocket(serverSocketContext, clientSocketContext));
            }
            catch
            {
                serverSocketContext.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Handles ServerStarted messages. It happens when service opened a new "server" connection back to us.
        /// </summary>
        /// <param name="clientSocketContext">The client socket context.</param>
        /// <returns></returns>
        private async Task HandleMessageServerStarted(SocketContext clientSocketContext)
        {
            var guid = await clientSocketContext.ReadStream.ReadGuidAsync();

            // Notify any waiter that a server with given GUID is available
            TaskCompletionSource<SocketContext> serverSocketTCS;
            lock (pendingServers)
            {
                if (!pendingServers.TryGetValue(guid, out serverSocketTCS))
                {
                    Log.Error("Could not find a matching server Guid");
                    clientSocketContext.Dispose();
                    return;
                }

                pendingServers.Remove(guid);
            }

            serverSocketTCS.TrySetResult(clientSocketContext);
        }

        /// <summary>
        /// Handles ServiceProvideServer messages. It allows service to publicize what "server" they can instantiate.
        /// </summary>
        /// <param name="clientSocketContext">The client socket context.</param>
        /// <returns></returns>
        private async Task HandleMessageServiceProvideServer(SocketContext clientSocketContext)
        {
            var url = await clientSocketContext.ReadStream.ReadStringAsync();
            TaskCompletionSource<SocketContext> service;

            lock (registeredServices)
            {
                if (!registeredServices.TryGetValue(url, out service))
                {
                    service = new TaskCompletionSource<SocketContext>();
                    registeredServices.Add(url, service);
                }

                service.TrySetResult(clientSocketContext);
            }

            // TODO: Handle server disconnections
            //clientSocketContext.Disconnected += 
        }

        private async Task ForwardSocket(SocketContext source, SocketContext target)
        {
            var buffer = new byte[1024];
            while (true)
            {
                var bufferLength = await source.ReadStream.ReadAsync(buffer, 0, buffer.Length);
                if (bufferLength == 0)
                    throw new IOException("Socket closed");
                await target.WriteStream.WriteAsync(buffer, 0, bufferLength);
                await target.WriteStream.FlushAsync();
            }
        }
    }
}