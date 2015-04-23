// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
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

        private object sendLock = new object();
        private bool isServer;
        private NetworkStream socketStream;
        private BinaryReader socketBinaryReader;
        private BinaryWriter socketBinaryWriter;
        private readonly ManualResetEvent allDone = new ManualResetEvent(false);
        private readonly Dictionary<int, TaskCompletionSource<SocketMessage>> packetCompletionTasks = new Dictionary<int, TaskCompletionSource<SocketMessage>>();

        Dictionary<Type, Tuple<Action<object>, bool>> packetHandlers = new Dictionary<Type, Tuple<Action<object>, bool>>();

        public Action<SocketContext> Connected;

        public void AddPacketHandler<T>(Action<T> handler, bool oneTime = false)
        {
            lock (packetHandlers)
            {
                packetHandlers.Add(typeof(T), Tuple.Create<Action<object>, bool>((obj) => handler((T)obj), oneTime));
            }
        }

        public void Send(object obj)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinarySerializationWriter(memoryStream);
            binaryWriter.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
            binaryWriter.SerializeExtended(obj, ArchiveMode.Serialize, null);
            var memoryBuffer = memoryStream.ToArray();

            // Make sure everything is sent at once
            lock (sendLock)
            {
                socketBinaryWriter.Write(memoryBuffer.Length);

                // Chunk it into block of 1024 bytes (not sure why but had some problem when doing send bigger than 3k would end up in buffer filled with 0, maybe Mono issue?)
                for (int i = 0; i < (memoryBuffer.Length + 1023)/1024; ++i)
                    socketStream.Write(memoryStream.GetBuffer(), i*1024, Math.Min(1024, memoryBuffer.Length - i*1024));
            }
        }

        public void StartServer(int port)
        {
            new Thread(SafeAction.Wrap(() => ServerThread(port))).Start();
        }

        public async Task StartClient(IPAddress address, int port)
        {
            var clientDone = new TaskCompletionSource<bool>();

            // note: we wrap in a thread because we use Connect (not async) and then MessageLoop is sync.
            // we should switch to ConnectAsync, and start thread only if MessageLoop is reached
            new Thread(() =>
            {
                SafeAction.Wrap(() => ClientThread(address, port).Wait())();
                clientDone.TrySetResult(true);
            }).Start();

            await clientDone.Task;
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
        
        async Task ClientThread(IPAddress address, int port)
        {
            // Try to connect
            while (true)
            {
                try
                {
                    var localEP = new IPEndPoint(address, port);

                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.NoDelay = true;
                    socket.Connect(localEP);

                    SetSocketStream(new NetworkStream(socket));

                    var ack = socketBinaryReader.ReadInt32();
                    if (ack != MagicAck)
                        throw new InvalidOperationException("Invalid ack");

                    if (Connected != null)
                        Connected(this);

                    break;
                }
                catch (Exception)
                {
                    // Mute connection errors
                }

                await Task.Delay(100);
            }

            // Start message loop
            try
            {
                MessageLoop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void ServerThread(int port)
        {
            //var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //var localEP = new IPEndPoint(ipHostInfo.AddressList[0], 11000);
            var localEP = new IPEndPoint(IPAddress.Any, port);

            var listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.NoDelay = true;
            try
            {
                listener.Bind(localEP);
                listener.Listen(10);
                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();

                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);

                var clientSocketContext = new SocketContext();
                clientSocketContext.SetSocketStream(new NetworkStream(handler));
                clientSocketContext.isServer = true;

                // Do an ack
                clientSocketContext.socketBinaryWriter.Write(MagicAck);

                if (Connected != null)
                    Connected(clientSocketContext);

                clientSocketContext.MessageLoop();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void MessageLoop()
        {
            while (true)
            {
                //var obj = formatter.Deserialize(socketStream);
                var remaining = socketBinaryReader.ReadInt32();
                var buffer = new byte[remaining];
                int offset = 0;
                while (remaining > 0)
                {
                    int read = socketStream.Read(buffer, offset, remaining);
                    remaining -= read;
                    offset += read;
                }
                var binaryReader = new BinarySerializationReader(new MemoryStream(buffer));
                object obj = null;
                binaryReader.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
                binaryReader.SerializeExtended<object>(ref obj, ArchiveMode.Deserialize, null);
                if (obj is SocketMessage)
                {
                    var socketMessage = (SocketMessage)obj;
                    ProcessMessage(socketMessage);
                }
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

        void SetSocketStream(NetworkStream socketStream)
        {
            this.socketStream = socketStream;
            socketBinaryReader = new BinaryReader(this.socketStream);
            socketBinaryWriter = new BinaryWriter(this.socketStream);
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
#endif