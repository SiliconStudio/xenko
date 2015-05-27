using System;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    public abstract class RouterServiceServer
    {
        private string address;
        private int port;

        private string url;

        protected RouterServiceServer(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Tries to connect. Blocks until connection fails or happens (if connection happens, it will launch the message loop in a separate unobserved Task).
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public async Task TryConnect(string address, int port)
        {
            this.address = address;
            this.port = port;

            var socketContext = CreateSocketContext();

            // Wait for a connection to be possible on adb forwarded port
            await socketContext.StartClient(address, port);
        }

        private SocketContext CreateSocketContext()
        {
            var socketContext = new SocketContext();
            socketContext.Connected = async (clientSocketContext) =>
            {
                // Register service server
                await socketContext.WriteStream.Write7BitEncodedInt((int)RouterMessage.ServiceProvideServer);
                await socketContext.WriteStream.WriteStringAsync(url);
                await socketContext.WriteStream.FlushAsync();

                while (true)
                {
                    var routerMessage = (RouterMessage)await socketContext.ReadStream.Read7BitEncodedInt();

                    switch (routerMessage)
                    {
                        case RouterMessage.ServiceRequestServer:
                        {
                            var guid = await clientSocketContext.ReadStream.ReadGuidAsync();

                            // Spawn actual server
                            var realServerSocketContext = new SocketContext();
                            realServerSocketContext.Connected = async (clientSocketContext2) =>
                            {
                                // Write connection string
                                await clientSocketContext2.WriteStream.Write7BitEncodedInt((int)RouterMessage.ServerStarted);
                                await clientSocketContext2.WriteStream.WriteGuidAsync(guid);
                                await clientSocketContext2.WriteStream.FlushAsync();

                                // Delegate next steps to actual server
                                RunServer(clientSocketContext2);
                            };
                            await realServerSocketContext.StartClient(address, port);
                            break;
                        }
                        default:
                            Console.WriteLine("Router: Unknown message: {0}", routerMessage);
                            throw new ArgumentOutOfRangeException();
                    }
                }
            };

            return socketContext;
        }

        protected abstract void RunServer(SocketContext clientSocketContext);
    }
}