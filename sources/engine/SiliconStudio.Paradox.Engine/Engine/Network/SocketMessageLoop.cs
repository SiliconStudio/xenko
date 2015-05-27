using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Engine.Network
{
    public class SocketMessageLoop
    {
        private SocketContext context;
        private bool isServer;

        private SemaphoreSlim sendLock = new SemaphoreSlim(1);
        private readonly Dictionary<int, TaskCompletionSource<SocketMessage>> packetCompletionTasks = new Dictionary<int, TaskCompletionSource<SocketMessage>>();
        private Dictionary<Type, Tuple<Action<object>, bool>> packetHandlers = new Dictionary<Type, Tuple<Action<object>, bool>>();

        public SocketMessageLoop(SocketContext context, bool isServer)
        {
            this.context = context;
            this.isServer = isServer;
        }

        public SocketContext Context
        {
            get { return context; }
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

        public void AddPacketHandler<T>(Action<T> handler, bool oneTime = false)
        {
            lock (packetHandlers)
            {
                packetHandlers.Add(typeof(T), Tuple.Create<Action<object>, bool>((obj) => handler((T)obj), oneTime));
            }
        }

        public async Task Send(object obj)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinarySerializationWriter(memoryStream);
            binaryWriter.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
            binaryWriter.SerializeExtended(obj, ArchiveMode.Serialize, null);

            var memoryBuffer = memoryStream.ToArray();

            // Make sure everything is sent at once
            await sendLock.WaitAsync();
            try
            {
                await context.WriteStream.Write7BitEncodedInt((int)memoryStream.Length);
                await context.WriteStream.WriteAsync(memoryBuffer, 0, (int)memoryStream.Length);
                await context.WriteStream.FlushAsync();
            }
            finally
            {
                sendLock.Release();
            }
        }

        public async Task MessageLoop()
        {
            try
            {
                while (true)
                {
                    // Get next packet size
                    var bufferSize = await context.ReadStream.Read7BitEncodedInt();

                    // Get next packet data (until complete)
                    var buffer = new byte[bufferSize];
                    await context.ReadStream.ReadAllAsync(buffer, 0, bufferSize);

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
                context.DisposeSocket();
                throw;
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