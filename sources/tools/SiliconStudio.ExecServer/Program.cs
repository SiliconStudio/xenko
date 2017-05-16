// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.ExecServer
{
    internal class Program
    {
        /// <summary>
        /// Main entry point for ExecServer. Add an attribute to notify that the server is hosting multiple domains using same assemblies.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>Status</returns>
        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static int Main(string[] args)
        {
            var serverApp = new ExecServerApp();
            return serverApp.Run(args);
        }
    }
}
