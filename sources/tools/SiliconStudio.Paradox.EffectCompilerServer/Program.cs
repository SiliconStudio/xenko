// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Reflection;
using Mono.Options;
using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.EffectCompilerServer
{
    class Program
    {
        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            int exitCode = 0;

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2015 Silicon Studio Corporation. All Rights Reserved",
                    "Paradox Effect Compiler Server - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0}", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
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

                var effectCompilerServer = new EffectCompilerServer();
                effectCompilerServer.TryConnect("127.0.0.1", RouterClient.DefaultPort);

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