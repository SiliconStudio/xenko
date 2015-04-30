// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME

using System;
using System.IO;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.Shaders.Compiler.Internals
{
    internal class DownloadFileQuery : SocketMessage
    {
        public string Url { get; set; }
    }

    internal class FileExistsQuery : SocketMessage
    {
        public string Url { get; set; }
    }

    internal class FileExistsAnswer : SocketMessage
    {
        public bool FileExists { get; set; }
    }

    internal class DownloadFileAnswer : SocketMessage
    {
        public byte[] Data { get; set; }
    }

    internal class UploadFilePacket
    {
        public string Url { get; set; }
        public byte[] Data { get; set; }
    }

    public class NetworkVirtualFileProvider : VirtualFileProviderBase
    {
        private SocketContext socketContext;

        public NetworkVirtualFileProvider(SocketContext socketContext, string remoteUrl) : base(null)
        {
            this.socketContext = socketContext;
            RemoteUrl = remoteUrl;
            if (!RemoteUrl.EndsWith(VirtualFileSystem.DirectorySeparatorChar.ToString()))
                RemoteUrl += VirtualFileSystem.DirectorySeparatorChar;
        }

        public string RemoteUrl { get; private set; }

        public static void RegisterServer(SocketContext socketContext)
        {
            socketContext.AddPacketHandler<DownloadFileQuery>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Open, VirtualFileAccess.Read);
                    var data = new byte[stream.Length];
                    await stream.ReadAsync(data, 0, data.Length);
                    stream.Close();
                    socketContext.Send(new DownloadFileAnswer { StreamId = packet.StreamId, Data = data });
                });

            socketContext.AddPacketHandler<UploadFilePacket>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Create, VirtualFileAccess.Write);
                    await stream.WriteAsync(packet.Data, 0, packet.Data.Length);
                    stream.Close();
                });

            socketContext.AddPacketHandler<FileExistsQuery>(
                async (packet) =>
                    {
                        var fileExists = await VirtualFileSystem.FileExistsAsync(packet.Url);
                        socketContext.Send(new FileExistsAnswer { StreamId = packet.StreamId, FileExists = fileExists });
                    });
        }

        public override string GetAbsolutePath(string path)
        {
            return RemoteUrl + path;
        }

        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamFlags = StreamFlags.None)
        {
            switch (access)
            {
                case VirtualFileAccess.Write:
                    return new NetworkWriteStream(socketContext, RemoteUrl + url);
                case VirtualFileAccess.Read:
                    var downloadFileAnswer = (DownloadFileAnswer)socketContext.SendReceiveAsync(new DownloadFileQuery { Url = RemoteUrl + url }).Result;
                    return new MemoryStream(downloadFileAnswer.Data);
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool FileExists(string url)
        {
            var fileExistsAnswer = (FileExistsAnswer)socketContext.SendReceiveAsync(new FileExistsQuery { Url = RemoteUrl + url }).Result;
            return fileExistsAnswer.FileExists;
        }

        internal class NetworkWriteStream : VirtualFileStream
        {
            private string url;
            private SocketContext socketContext;
            private MemoryStream memoryStream;

            public NetworkWriteStream(SocketContext socketContext, string url)
                : base(new MemoryStream())
            {
                this.memoryStream = (MemoryStream)InternalStream;
                this.url = url;
                this.socketContext = socketContext;
            }

            protected override void Dispose(bool disposing)
            {
                socketContext.Send(new UploadFilePacket { Url = url, Data = memoryStream.ToArray() });
                base.Dispose(disposing);
            }
        }
    }
}
#endif