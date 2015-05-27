// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Sockets.Plugin;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace SiliconStudio.Paradox.Engine.Network
{
    /// <summary>
    /// Manages socket connection and low-level communication.
    /// High-level communication is supposed to happen in <see cref="SocketMessageLoop"/>.
    /// </summary>
    public class SocketContext : IDisposable
    {
        private const int ServerMagicAck = 0x35AABBCC;
        private const int ClientMagicAck = 0x24BB35CC;

        private TcpSocketClient socket;
        private bool isConnected;

        public Stream ReadStream
        {
            get { return socket.ReadStream; }
        }

        public Stream WriteStream
        {
            get { return socket.WriteStream; }
        }

        public string RemoteAddress
        {
            get { return socket.RemoteAddress; }
        }

        public int RemotePort
        {
            get { return socket.RemotePort; }
        }

        // Called on a succesfull connection
        public Action<SocketContext> Connected;

        // Called if there is a socket failure (after ack handshake)
        public Action<SocketContext> Disconnected;

        public void Dispose()
        {
            DisposeSocket();
        }

        public async Task StartServer(int port, bool singleConnection)
        {
            //var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //var localEP = new IPEndPoint(ipHostInfo.AddressList[0], 11000);
            var listener = new TcpSocketListener(2048);
            //listener.NoDelay = true;

            listener.ConnectionReceived = async (sender, args) =>
            {
                var clientSocketContext = new SocketContext();

                try
                {
                    // Stop listening if we accept only a single connection
                    if (singleConnection)
                        await listener.StopListeningAsync();

                    clientSocketContext.SetSocket((TcpSocketClient)args.SocketClient);

                    // Do an ack
                    await clientSocketContext.socket.WriteStream.WriteInt32Async(ServerMagicAck);
                    await clientSocketContext.socket.WriteStream.FlushAsync();
                    var ack = await clientSocketContext.socket.ReadStream.ReadInt32Async();
                    if (ack != ClientMagicAck)
                        throw new InvalidOperationException("Invalid ack");

                    if (Connected != null)
                        Connected(clientSocketContext);

                    clientSocketContext.isConnected = true;
                }
                catch (Exception)
                {
                    clientSocketContext.DisposeSocket();
                }
            };

            // Start listening
            await listener.StartListeningAsync(port);
        }

        public async Task StartClient(string address, int port)
        {
            var socket = new TcpSocketClient(2048);

            try
            {
                await socket.ConnectAsync(address, port);

                SetSocket(socket);
                //socket.NoDelay = true;

                // Do an ack
                await socket.WriteStream.WriteInt32Async(ClientMagicAck);
                await socket.WriteStream.FlushAsync();
                var ack = await socket.ReadStream.ReadInt32Async();
                if (ack != ServerMagicAck)
                    throw new InvalidOperationException("Invalid ack");

                if (Connected != null)
                    Connected(this);

                isConnected = true;
            }
            catch (Exception)
            {
                DisposeSocket();
                throw;
            }
        }
        
        void SetSocket(TcpSocketClient socket)
        {
            this.socket = socket;
        }

        internal void DisposeSocket()
        {
            if (this.socket != null)
            {
                if (isConnected)
                {
                    isConnected = false;
                    if (Disconnected != null)
                        Disconnected(this);
                }

                this.socket.Dispose();
                this.socket = null;
            }
        }
    }
}