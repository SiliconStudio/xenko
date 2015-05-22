// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.Engine.Network
{
    static class SocketExtensions
    {
        public static async Task ReadAllAsync(this Stream socket, byte[] buffer, int offset, int size)
        {
            while (size > 0)
            {
                int read = await socket.ReadAsync(buffer, offset, size);
                if (read == 0)
                    throw new InvalidOperationException("Could not read from socket");
                size -= read;
                offset += read;
            }
        }

        public static async Task WriteInt32Async(this Stream socket, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            await socket.WriteAsync(buffer, 0, sizeof(int));
            await socket.FlushAsync();
        }

        public static async Task<Int32> ReadInt32Async(this Stream socket)
        {
            var buffer = new byte[sizeof(int)];
            await socket.ReadAllAsync(buffer, 0, sizeof(int));
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}