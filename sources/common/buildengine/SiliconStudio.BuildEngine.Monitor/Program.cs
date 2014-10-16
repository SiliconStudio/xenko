// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using System.Threading;

using Mono.Options;

namespace SiliconStudio.BuildEngine.Monitor
{
    static class Program
    {
        public static string MonitorPipeName = Builder.MonitorPipeName;

        [STAThread]
        [DebuggerNonUserCode]
        public static void Main(string[] args)
        {
            var p = new OptionSet
                {
                    { "monitor-pipe=", "Monitor pipe.", v => { if (!string.IsNullOrEmpty(v)) MonitorPipeName = v; } },
                };

            p.Parse(args);

            var instanceMutex = new Mutex(true, "Monitor-Mutex-" + MonitorPipeName);

            if (instanceMutex.WaitOne(0, true))
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }

            // Die silently if an instance is already running
        }
    }
}
