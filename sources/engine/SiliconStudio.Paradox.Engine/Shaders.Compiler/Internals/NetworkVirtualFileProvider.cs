// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
        private SocketMessageLoop socketMessageLoop;

        public NetworkVirtualFileProvider(SocketMessageLoop socketMessageLoop, string remoteUrl) : base(null)
        {
            this.socketMessageLoop = socketMessageLoop;
            RemoteUrl = remoteUrl;
            if (!RemoteUrl.EndsWith(VirtualFileSystem.DirectorySeparatorChar.ToString()))
                RemoteUrl += VirtualFileSystem.DirectorySeparatorChar;
        }

        public string RemoteUrl { get; private set; }

        public static void RegisterServer(SocketMessageLoop socketMessageLoop)
        {
            socketMessageLoop.AddPacketHandler<DownloadFileQuery>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Open, VirtualFileAccess.Read);
                    var data = new byte[stream.Length];
                    await stream.ReadAsync(data, 0, data.Length);
                    stream.Dispose();
                    socketMessageLoop.Send(new DownloadFileAnswer { StreamId = packet.StreamId, Data = data });
                });

            socketMessageLoop.AddPacketHandler<UploadFilePacket>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Create, VirtualFileAccess.Write);
                    await stream.WriteAsync(packet.Data, 0, packet.Data.Length);
                    stream.Dispose();
                });

            socketMessageLoop.AddPacketHandler<FileExistsQuery>(
                async (packet) =>
                    {
                        var fileExists = await VirtualFileSystem.FileExistsAsync(packet.Url);
                        socketMessageLoop.Send(new FileExistsAnswer { StreamId = packet.StreamId, FileExists = fileExists });
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
                    return new NetworkWriteStream(socketMessageLoop, RemoteUrl + url);
                case VirtualFileAccess.Read:
                    var downloadFileAnswer = (DownloadFileAnswer)socketMessageLoop.SendReceiveAsync(new DownloadFileQuery { Url = RemoteUrl + url }).Result;
                    return new MemoryStream(downloadFileAnswer.Data);
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool FileExists(string url)
        {
            var fileExistsAnswer = (FileExistsAnswer)socketMessageLoop.SendReceiveAsync(new FileExistsQuery { Url = RemoteUrl + url }).Result;
            return fileExistsAnswer.FileExists;
        }

        internal class NetworkWriteStream : VirtualFileStream
        {
            private string url;
            private SocketMessageLoop socketMessageLoop;
            private MemoryStream memoryStream;

            public NetworkWriteStream(SocketMessageLoop socketMessageLoop, string url)
                : base(new MemoryStream())
            {
                this.memoryStream = (MemoryStream)InternalStream;
                this.url = url;
                this.socketMessageLoop = socketMessageLoop;
            }

            protected override void Dispose(bool disposing)
            {
                socketMessageLoop.Send(new UploadFilePacket { Url = url, Data = memoryStream.ToArray() });
                base.Dispose(disposing);
            }
        }
    }
}