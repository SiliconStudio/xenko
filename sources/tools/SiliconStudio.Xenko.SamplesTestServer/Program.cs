// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using SiliconStudio.Xenko.Engine.Network;

namespace SiliconStudio.Xenko.SamplesTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var samplesServer = new SamplesTestServer();
            try
            {
                samplesServer.TryConnect("127.0.0.1", RouterClient.DefaultPort).Wait();
            }
            catch
            {
                return;
            }          

            // Forbid process to terminate (unless ctrl+c)
            while (true)
            {
                Console.Read();
                Thread.Sleep(100);
            }
        }
    }
}
