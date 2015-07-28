// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.ExecServer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var serverApp = new ExecServerApp();
            return serverApp.Run(args);
        }
    }
}