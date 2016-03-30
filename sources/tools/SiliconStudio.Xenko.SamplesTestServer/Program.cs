// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
