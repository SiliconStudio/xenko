// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Engine.Network
{
    // Temporary socket class
    // TODO: redesign it, convert to SendAsync/ReceiveAsync, etc...
    public class SocketContext
    {
        private const int MagicAck = 0x35AABBCC;

        private SemaphoreSlim sendLock = new SemaphoreSlim(1);
        private bool isServer;
        private bool isConnected;
        private TcpSocketClient socket;
        private readonly Dictionary<int, TaskCompletionSource<SocketMessage>> packetCompletionTasks = new Dictionary<int, TaskCompletionSource<SocketMessage>>();

        Dictionary<Type, Tuple<Action<object>, bool>> packetHandlers = new Dictionary<Type, Tuple<Action<object>, bool>>();

        // Called on a succesfull connection
        public Action<SocketContext> Connected;

        // Called if there is a socket failure (after ack handshake)
        public Action<SocketContext> Disconnected;

        public void AddPacketHandler<T>(Action<T> handler, bool oneTime = false)
        {
            lock (packetHandlers)
            {
                packetHandlers.Add(typeof(T), Tuple.Create<Action<object>, bool>((obj) => handler((T)obj), oneTime));
            }
        }

        public async void Send(object obj)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinarySerializationWriter(memoryStream);
            binaryWriter.Write(0); // Write empty size
            binaryWriter.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
            binaryWriter.SerializeExtended(obj, ArchiveMode.Serialize, null);
            
            // Update size
            memoryStream.Position = 0;
            binaryWriter.Write((int)memoryStream.Length - 4);

            var memoryBuffer = memoryStream.ToArray();

            // Make sure everything is sent at once
            await sendLock.WaitAsync();
            try
            {
                await socket.WriteStream.WriteAsync(memoryBuffer, 0, (int)memoryStream.Length);
                await socket.WriteStream.FlushAsync();
            }
            finally
            {
                sendLock.Release();
            }
        }

        public async Task StartServer(int port, bool singleConnection)
        {
            //var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //var localEP = new IPEndPoint(ipHostInfo.AddressList[0], 11000);
            var listener = new TcpSocketListener();
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
                    clientSocketContext.isServer = true;

                    // Do an ack
                    await clientSocketContext.socket.WriteStream.WriteInt32Async(MagicAck);
                    var ack = await clientSocketContext.socket.ReadStream.ReadInt32Async();
                    if (ack != MagicAck)
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

        public async Task<SocketMessage> SendReceiveAsync(SocketMessage query)
        {
            var tcs = new TaskCompletionSource<SocketMessage>();
            query.StreamId = SocketMessage.NextStreamId + (isServer ? 0x4000000 : 0);
            lock (packetCompletionTasks)
            {
                packetCompletionTasks.Add(query.StreamId, tcs);
            }
            Send(query);
            return await tcs.Task;
        }
        
        public async Task StartClient(string address, int port)
        {
            var socket = new TcpSocketClient();

            try
            {
                await socket.ConnectAsync(address, port);

                SetSocket(socket);
                //socket.NoDelay = true;

                // Do an ack
                await socket.WriteStream.WriteInt32Async(MagicAck);
                var ack = await socket.ReadStream.ReadInt32Async();
                if (ack != MagicAck)
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
        
        public async Task MessageLoop()
        {
            try
            {
                while (true)
                {
                    // Get next packet size
                    var bufferSize = await socket.ReadStream.ReadInt32Async();

                    // Get next packet data (until complete)
                    var buffer = new byte[bufferSize];
                    await socket.ReadStream.ReadAllAsync(buffer, 0, bufferSize);

                    // Deserialize as an object
                    var binaryReader = new BinarySerializationReader(new MemoryStream(buffer));
                    object obj = null;
                    binaryReader.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
                    binaryReader.SerializeExtended<object>(ref obj, ArchiveMode.Deserialize, null);

                    // If it's a message, process it separately (StreamId)
                    if (obj is SocketMessage)
                    {
                        var socketMessage = (SocketMessage)obj;
                        ProcessMessage(socketMessage);
                    }

                    // Check if there is a specific handler for this packet type
                    bool handlerFound;
                    Tuple<Action<object>, bool> handler;
                    lock (packetHandlers)
                    {
                        handlerFound = packetHandlers.TryGetValue(obj.GetType(), out handler);

                        // one-time handler
                        if (handlerFound && handler.Item2)
                        {
                            packetHandlers.Remove(obj.GetType());
                        }
                    }

                    if (handlerFound)
                    {
                        handler.Item1(obj);
                    }
                }
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

        void DisposeSocket()
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

        void ProcessMessage(SocketMessage socketMessage)
        {
            TaskCompletionSource<SocketMessage> tcs;
            lock (packetCompletionTasks)
            {
                packetCompletionTasks.TryGetValue(socketMessage.StreamId, out tcs);
                if (tcs != null)
                    packetCompletionTasks.Remove(socketMessage.StreamId);
            }
            if (tcs != null)
                tcs.TrySetResult(socketMessage);
        }
    }
}