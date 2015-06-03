// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    partial class Program
    {
        private static string IpOverUsbParadoxName = "ParadoxRouterServer";

        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            var windowsPhonePortMapping = false;
            int exitCode = 0;

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2015 Silicon Studio Corporation. All Rights Reserved",
                    "Paradox Router Server - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} command [options]*", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "register-windowsphone-portmapping", "Register Windows Phone IpOverUsb port mapping", v => windowsPhonePortMapping = true },
                };

            try
            {
                var commandArgs = p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                // Make sure path exists
                if (commandArgs.Count > 0)
                    throw new OptionException("This command expect no additional arguments", "");

                if (windowsPhonePortMapping)
                {
                    WindowsPhoneTracker.RegisterWindowsPhonePortMapping();
                    return 0;
                }

                // Enable console logging
                var consoleLogListener = new ConsoleLogListener { LogMode = ConsoleLogMode.Always };
                GlobalLogger.GlobalMessageLogged += consoleLogListener;

                if (!RouterHelper.RouterMutex.WaitOne(TimeSpan.Zero, true))
                {
                    Console.WriteLine("Another instance of Paradox Router is already running");
                    return -1;
                }

                var router = new Router();

                // Start server mode
                router.Listen(RouterClient.DefaultPort);

                // Start Android management thread
                new Thread(() => AndroidTracker.TrackDevices(router)).Start();

                // Start Windows Phone management thread
                new Thread(() => WindowsPhoneTracker.TrackDevices(router)).Start();

                // Forbid process to terminate (unless ctrl+c)
                while (true) Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: {1}", exeName, e);
                if (e is OptionException)
                    p.WriteOptionDescriptions(Console.Out);
                exitCode = 1;
            }

            return exitCode;
        }
    }
}