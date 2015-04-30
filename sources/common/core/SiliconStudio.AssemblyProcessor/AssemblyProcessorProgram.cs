// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Options;
using SiliconStudio.Core;

namespace SiliconStudio.AssemblyProcessor
{
    public class AssemblyProcessorProgram
    {
        public static readonly string ExeName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static void Main(string[] args)
        {
            bool showHelp;
            string outputFilePath;

            OptionSet p;
            List<string> inputFiles;
            var app = CreateAssemblyProcessorApp(args, out p, out showHelp, out outputFilePath, out inputFiles);
            if (showHelp)
            {
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (inputFiles.Count != 1)
            {
                p.WriteOptionDescriptions(Console.Out);
                ExitWithError("This tool requires one input file.");
            }

            var inputFile = inputFiles[0];

            // Add search path from input file
            app.SearchDirectories.Add(Path.GetDirectoryName(inputFile));

            // Load symbol file if it exists
            var symbolFile = Path.ChangeExtension(inputFile, "pdb");
            if (File.Exists(symbolFile))
            {
                app.UseSymbols = true;
            }

            // Setup output filestream
            if (outputFilePath == null)
            {
                outputFilePath = inputFile;
            }

            if (!app.Run(inputFile, outputFilePath))
            {
                ExitWithError();
            }
        }

        public static AssemblyProcessorApp CreateAssemblyProcessorApp(string[] args)
        {
            OptionSet p;
            bool showHelp;
            string outputFilePath;
            List<String> inputFiles;
            return CreateAssemblyProcessorApp(args, out p, out showHelp, out outputFilePath, out inputFiles);
        }

        public static AssemblyProcessorApp CreateAssemblyProcessorApp(string[] args, out OptionSet p, out bool showHelp, out string outputFilePath, out List<string> inputFiles)
        {
            bool localShowHelp = false;
            string localOutputFilePath = null;

            var app = new AssemblyProcessorApp();
            p = new OptionSet()
            {
                "Copyright (C) 2011-2012 Silicon Studio Corporation. All Rights Reserved",
                "Paradox Assembly Processor tool - Version: "
                +
                String.Format(
                    "{0}.{1}.{2}",
                    typeof(AssemblyProcessorProgram).Assembly.GetName().Version.Major,
                    typeof(AssemblyProcessorProgram).Assembly.GetName().Version.Minor,
                    typeof(AssemblyProcessorProgram).Assembly.GetName().Version.Build) + string.Empty,
                string.Format("Usage: {0} [options]* inputfile -o [outputfile]", ExeName),
                string.Empty,
                "=== Options ===",
                string.Empty,
                { "h|help", "Show this message and exit", v => localShowHelp = v != null },
                { "o|output=", "Output file name", v => localOutputFilePath = v },
                { "p|platform=", "The platform (Windows, Android, iOS)", v => app.Platform = (PlatformType)Enum.Parse(typeof(PlatformType), v) },
                { "t|targetFramework=", "The .NET target platform (platform specific)", v => app.TargetFramework = v },
                { "auto-notify-property", "Automatically implements INotifyPropertyChanged", v => app.AutoNotifyProperty = true },
                { "parameter-key", "Automatically initialize parameter keys in module static constructor", v => app.ParameterKey = true },
                { "rename-assembly=", "Rename assembly", v => app.NewAssemblyName = v },
                { "auto-module-initializer", "Execute function tagged with [ModuleInitializer] at module initialization (automatically enabled)", v => app.ModuleInitializer = true },
                { "serialization", "Generate serialiation assembly", v => app.SerializationAssembly = true },
                { "generate-user-doc", "Generate user documentation from XML file", v => app.GenerateUserDocumentation = true },
                { "d|directory=", "Additional search directory for assemblies", app.SearchDirectories.Add },
                { "a|assembly=", "Additional assembly (for now, it will add the assembly directory to search path)", v => app.SearchDirectories.Add(Path.GetDirectoryName(v)) },
                { "signkeyfile=", "Signing Key File", v => app.SignKeyFile = v },
                { "serializationProjectReferencePaths=", "Project reference paths to auto-load by serialization module initializer (separated by semi-colon)", v => app.SerializationProjectReferences.AddRange(v.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) }
            };

            showHelp = localShowHelp;
            outputFilePath = localOutputFilePath;
            inputFiles = p.Parse(args);
            return app;
        }

        private static void ExitWithError(string message = null)
        {
            if (message != null)
                Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
