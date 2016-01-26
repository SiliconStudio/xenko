// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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