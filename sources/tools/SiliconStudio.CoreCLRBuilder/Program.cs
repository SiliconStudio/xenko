// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

// CoreCLRBuilder is a multi-purpose tool for building and verifying a set of DLLs that will be
// used by CoreCLR. For this purpose, it uses the reference assemblies generated when building
// CoreFX (http://github.com/dotnet/corefx).


using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;
using Mono.Cecil;

namespace SiliconStudio.CoreCLRBuilder
{
    internal class Program
    {
        /// <summary>
        /// Entry point for the CoreCLRBuilder program. It builds the command line parser and if everything is fine executes the requested command.
        /// </summary>
        /// <param name="args">Command line arguments if any.</param>
        /// <returns></returns>
        [STAThread]
        private static int Main(string[] args)
        {
            ProgramParser app = new ProgramParser();
            app.Parse(args);
            if (app.IsParsingOk)
            {
                app.Run();
            }
            else
            {
                app.DisplayHelp();
            }
            return app.ExitCodeStatus;
        }

    }

    /// <summary>
    /// Parser for the command line arguments of the CoreCLRBuilder program.
    /// </summary>
    internal class ProgramParser {

#region Initialization
        /// <summary>
        /// Initialize current instance.
        /// </summary>
        public ProgramParser ()
        {
                // Circumvoluted way to get the executable path of CoreBuilder.
            foldersToCheck = new List<string>(10);
            options = new OptionSet {
                "Copyright (C) 2015-2015 Silicon Studio Corporation. All Rights Reserved",
                "Xenko Assembly Consistency Checker Tool - Version: "
                +
                String.Format(
                    "{0}.{1}.{2}",
                    typeof(ProgramParser).GetTypeInfo().Assembly.GetName().Version.Major,
                    typeof(ProgramParser).GetTypeInfo().Assembly.GetName().Version.Minor,
                    typeof(ProgramParser).GetTypeInfo().Assembly.GetName().Version.Build) + string.Empty,
                string.Empty,
                string.Format("Usage: {0} --check=directory [options]*", ExecutableName),
                string.Empty,
                 "=== General options ===",
                string.Empty,
                { "c|check=", "Check folder for consistency", v => foldersToCheck.Add (v) },
                { "b|build=", "Build reference assemblies", v =>
                    {
                        if (refAssemblyFolder == null)
                        {
                            refAssemblyFolder = v;
                        }
                        else
                        {
                            throw new OptionException("Duplicated option", "\"build\" option specified more than once!");
                        }
                    } },
                { "o|output=", "Output directory for reference assemblies", v => outputFolder = v},
                { "h|help", "Show this message and exit", v => showHelp = v != null },
                string.Empty,
            };
        }
#endregion

#region
        /// <summary>
        /// Name of current exucutable.
        /// </summary>
        public static string ExecutableName { get; } = Path.GetFileName(typeof(ProgramParser).GetTypeInfo().Assembly.GetModules()[0].FullyQualifiedName);
#endregion

#region Status report
        /// <summary>
        /// Was last call to <see cref="Parse"/> successful?
        /// </summary>
        public bool IsParsingOk { get; set; }

        /// <summary>
        /// Exit code status for current run.
        /// </summary>
        public int ExitCodeStatus { get; set; }
#endregion

#region Operations
        /// <summary>
        /// Parse command line arguments <see cref="args"/>.
        /// </summary>
        /// <param name="args"></param>
        public virtual void Parse(string[] args)
        {
            Contract.Requires(args != null, "Argument should be set!");

            IsParsingOk = false;
            ExitCodeStatus = 0;

            try
            {
                if (args.Length != 0)
                {
                    var commandArgs = options.Parse(args);
                        // If we still have some unparsed arguments in commandArgs, clearly
                        // this is not a valid command line argument.
                    IsParsingOk = commandArgs.Count == 0;
                }
            }
            catch (Exception e)
            {
                var o = e as OptionException;
                if (o != null)
                {
                    DisplayError(string.Format("An error occurred during parsing.\nException of type {0} was raised due to {1}.\nStack trace is:\n{2}", o.GetType().ToString(), o.OptionName, o.StackTrace));
                }
                ExitCodeStatus = -1;
            }
        }

        /// <summary>
        /// Display Help for current program.
        /// </summary>
        public virtual void DisplayHelp()
        {
            Console.WriteLine();
            options.WriteOptionDescriptions(Console.Out);
        }
        
        /// <summary>
        /// Run program with command line arguments provided in <see cref="Parse"/>.
        /// </summary>
        public virtual void Run()
        {
            Contract.Requires(IsParsingOk, "Command line argument should be valid");

                // Initialize it to the default value.
            ExitCodeStatus = 0;

            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
            }
            else
            {
                foreach (var folder in foldersToCheck)
                {
                    CheckDirectory(folder);
                }

                if ((refAssemblyFolder != null) && (outputFolder != null))
                {
                    BuildReferenceAssemblies(refAssemblyFolder, outputFolder);
                }
            }
        }
#endregion

#region Implementation Access
        /// <summary>
        /// Reference to argument parser.
        /// </summary>
        private OptionSet options;

        /// <summary>
        /// Is showing help requested?
        /// </summary>
        private bool showHelp;

        /// <summary>
        /// List of Folders to check.
        /// </summary>
        private List<string> foldersToCheck;

        /// <summary>
        /// Store the location where the reference assemblies are stored.
        /// </summary>
        private string refAssemblyFolder;
        
        /// <summary>
        /// Output location where reference assemblies will be copied to build a self contained and consistent set
        /// of reference assemblies.
        /// </summary>
        private string outputFolder;
#endregion

#region Implementation
        /// <summary>
        /// Process all assemblies in directory specified by <see cref="path"/> and ensures that all references are matching (no version mismatch).
        /// </summary>
        /// <param name="path">Location where assemblies will be checked.</param>
        private void CheckDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            Dictionary<string, AssemblyDefinition> assemblyTable = new Dictionary<string, AssemblyDefinition>(
                StringComparer.CurrentCultureIgnoreCase);
            Dictionary<string, AssemblyDefinition> assemblyCloseMatchTable = new Dictionary<string, AssemblyDefinition>(
                StringComparer.CurrentCultureIgnoreCase);

            if (dir.Exists)
            {
                // First we load all the assemblies and get their full qualified name and store those in assemblyTable and assemblyCloseMatchTable.
                foreach (var file in EnumerateDlls(dir))
                {
                    AssemblyDefinition ass = SafeReadAssembly(file.FullName);
                    if (ass != null)
                    {
                        assemblyTable.Add(ass.FullName, ass);
                        assemblyCloseMatchTable.Add(ass.Name.Name, ass);
                    }
                }

                // Check that all references are found in current directory.
                foreach (var entry in assemblyTable)
                {
                    foreach (var module in entry.Value.Modules)
                    {
                        foreach (var refAssembly in module.AssemblyReferences)
                        {
                            if (!assemblyTable.ContainsKey(refAssembly.FullName))
                            {
                                AssemblyDefinition otherAss;
                                assemblyCloseMatchTable.TryGetValue(refAssembly.Name, out otherAss);
                                if (otherAss == null)
                                {
                                    Console.WriteLine(string.Format("Assembly {0} refers to {1} which cannot be found.\n", entry.Key, refAssembly.FullName));
                                }
                                else if (otherAss.Name.Version <= refAssembly.Version)
                                {
                                    Console.WriteLine(string.Format("Assembly {0} refers to {1} but we could only find:\n\t {2} cannot be found.\n", entry.Key, refAssembly.FullName, otherAss.FullName));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DisplayError(string.Format("Directory \"{0}\" does not exist.", dir.ToString()));
                ExitCodeStatus = -1;
            }
        }

        /// <summary>
        /// Given a location <see cref="refAssemblyFolder"/> where reference assemblies are in CoreFX, copy in <see cref="outputFolder"/>
        /// all assemblies to have a folder where all the assemblies are located and making sure they are consistent.
        /// </summary>
        /// <param name="refAssemblyFolder">Location where the reference assemblies are located.</param>
        /// <param name="outputFolder">Location where the selected reference assemblies will be copied.</param>
        private void BuildReferenceAssemblies(string refAssemblyFolder, string outputFolder)
        {
            Contract.Requires(refAssemblyFolder != null, "Reference Assemblies folder should be set!");
            Contract.Requires(outputFolder != null, "Output directory should be set!");

            DirectoryInfo topDir = new DirectoryInfo(refAssemblyFolder);
            DirectoryInfo outputDir = new DirectoryInfo(outputFolder);

            if (!topDir.Exists)
            {
                DisplayError(string.Format("Directory \"{0}\" does not exist.", refAssemblyFolder));
                ExitCodeStatus = -1;
            }

            if (!outputDir.Exists)
            {
                DisplayError(string.Format("Directory \"{0}\" does not exist.", outputFolder));
                ExitCodeStatus = -1;
            }

            if (ExitCodeStatus == 0)
            {
                    // The structure of topDir is always of the form: AssemblyName\VERSION\AssemblyName.dll
                    // Depending on whether we want the very latest of reference assemblies or not.
                foreach (var assemblyDir in topDir.EnumerateDirectories())
                {
                    Version maxVersion = null;
                    IEnumerable<FileInfo> assemblies = null;
                    foreach (var versionDir in assemblyDir.EnumerateDirectories())
                    {
                        Version version = new Version(versionDir.Name);

                            // Take all the assemblies we found that matched the lowest version.
                        if ((maxVersion == null) || (version > maxVersion))
                        {
                            maxVersion = version;
                            assemblies = EnumerateDlls(versionDir);
                        }
                    }

                    if (assemblies == null)
                    {
                        DisplayWarning(string.Format("Found no assembly for {0}", assemblyDir.Name));
                    }
                    else
                    {
                        foreach (var assembly in assemblies)
                        {
                            string destFileName = Path.Combine(outputFolder, assembly.Name);
                            FileInfo destFile = new FileInfo(destFileName);
                            if (!destFile.Exists)
                            {
                                assembly.CopyTo(Path.Combine(outputFolder, assembly.Name));
                            }
                            else
                            {
                                DisplayWarning(string.Format ("Assembly {0} already present in {1}", assembly.FullName, outputFolder));
                            }
                        }
                    }
                }
            }
        }
#endregion

#region Implementation: Helpers
        /// <summary>
        /// Load file <see cref="fileName"/> as an assembly and if it succeeds return the AssemblyDefinition instance.
        /// </summary>
        /// <param name="fileName">Name of file to load as an assembly.</param>
        /// <returns></returns>
        private AssemblyDefinition SafeReadAssembly(string fileName)
        {
            try
            {
                return AssemblyDefinition.ReadAssembly(fileName);
            }
            catch
            {
                    // Not a valid Assembly, we do not care.
                return null;
            }
        }

        /// <summary>
        /// Enumerator that only cares about DLL files located in <see cref="dir"/> and nothing else.
        /// </summary>
        /// <param name="dir">Directory where to look for DLL files.</param>
        /// <returns></returns>
        private IEnumerable<FileInfo> EnumerateDlls(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles())
            {
                if (file.Extension.ToLower() == ".dll")
                {
                    yield return file;
                }
            }
        }

        /// <summary>
        /// Display error message <see cref="errorMsg"/>
        /// </summary>
        /// <param name="errorMsg">Error message to display.</param>
        private void DisplayError(string errorMsg)
        {
            Console.WriteLine(string.Format("Error in {0}: {1}", ExecutableName, errorMsg));
        }

        /// <summary>
        /// Display warning message <see cref="errorMsg"/>
        /// </summary>
        /// <param name="errorMsg">Error message to display.</param>
        private void DisplayWarning(string errorMsg)
        {
            Console.WriteLine(string.Format("Warning in {0}: {1}", ExecutableName, errorMsg));
        }
#endregion

    }
}
