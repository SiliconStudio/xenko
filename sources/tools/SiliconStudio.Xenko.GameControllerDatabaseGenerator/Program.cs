// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Reflection;
using Mono.Options;

namespace SiliconStudio.Xenko.GameControllerDatabaseGenerator
{
    /// <summary>
    /// This program parses a file in the format of an SDL2 game controller mapping and generates Xenko game controller mapping for these so they can be used when not using SDL as well.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

            var p = new OptionSet
                {
                    "Copyright (C) 2016 Silicon Studio Corporation. All Rights Reserved",
                    "Xenko Game Controller Database Generator - Version: "
                    +
                    $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}" + string.Empty,
                    $"Usage: {exeName} [input file] [output file]",
                    $"Where the input file is a valid SDL2 game controller configuration file",
                    "=== General options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                };

            try
            {
                var commandArgs = p.Parse(args);
                if (showHelp || commandArgs.Count < 2)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                GamePadLayout layout = new GamePadLayout(commandArgs[0]);

                using (var target = File.Open(commandArgs[1], FileMode.Create))
                using (var writer = new StreamWriter(target))
                {
                    writer.Write((string)layout.TransformText());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"{exeName}: {e}");
                if (e is OptionException)
                    p.WriteOptionDescriptions(Console.Out);
                return 1;
            }

            return 0;
        }
    }
}
