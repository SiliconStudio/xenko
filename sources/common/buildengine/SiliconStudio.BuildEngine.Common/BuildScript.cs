// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.CSharp;

namespace SiliconStudio.BuildEngine
{
    public class BuildScript
    {
        /// <summary>
        /// Indicate the location of this <see cref="BuildScript"/>.
        /// </summary>
        public string ScriptPath { get; private set; }

        /// <summary>
        /// Indicate source base directory, which is used as working directory for the Build Engine. The path of every file accessed by the build script must be relative to this directory.
        /// </summary>
        public string SourceBaseDirectory { get; private set; }

        /// <summary>
        /// List of every source folders used in this script, relative to the <see cref="SourceBaseDirectory"/>. The key describe the variable name, and the value is the relative path.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> SourceFolders { get; private set; }

        /// <summary>
        /// Indicate the build directory. This is where the Build Engine will write its cache information as well as the asset database (if used and if <see cref="OutputDirectory"/> is null)
        /// </summary>
        public string BuildDirectory { get; private set; }

        /// <summary>
        /// Indicate the output directory. This is where the Build Engine will output the asset database files. If null, <see cref="BuildDirectory"/> value is used.
        /// </summary>
        public string OutputDirectory { get; private set; }

        /// <summary>
        /// Indicate wheither the BuildScript generated errors while it was compiled or loaded
        /// </summary>
        public string MetadataDatabaseDirectory { get; private set; }

        /// <summary>
        /// Indicate wheither the BuildScript generated errors while it was compiled
        /// </summary>
        public bool HasErrors { get { return GetErrors().Any(); } }

        /// <summary>
        /// Indicate wheither the BuildScript has been compiled
        /// </summary>
        public bool IsCompiled { get; private set; }

        private string source;
        private CompilerResults compilerResult;
        private readonly List<string> parsingErrors = new List<string>();
        private readonly List<string> parsingWarnings = new List<string>();
        private readonly BuildParameterCollection parameters = new BuildParameterCollection();

        private BuildScript(string xenkoSdkDir)
        {
            SourceFolders = new Dictionary<string, string>();
            ((Dictionary<string, string>)SourceFolders).Add("XenkoSdkDir", xenkoSdkDir);
        }

        public static BuildScript LoadFromFile(string xenkoSdkDir, string filePath)
        {
            var script = new BuildScript(xenkoSdkDir) { ScriptPath = filePath, source = File.ReadAllText(filePath) };
            return script;
        }

        private static string StripQuotes(string str)
        {
            if (str.StartsWith("\"") && str.EndsWith("\"") && str.Length >= 2)
                return str.Substring(1, str.Length - 2);
            return str;
        }

        private void ParseParameters()
        {
            // Simple parameters, following this syntax: // #[Name] [Value]
            var parameterRegex = new Regex(@"^//\s*#(\w+)\s+(""[^""]+?""|[\w.\-/\\]+)\s*$", RegexOptions.Multiline);
            foreach (Match match in parameterRegex.Matches(source))
            {
                parameters.Add(StripQuotes(match.Groups[1].Value), StripQuotes(match.Groups[2].Value));
            }
            // Key-Value parameters, following this syntax: // #[Name] [Key] [Value]
            parameterRegex = new Regex(@"^//\s*#(\w+)\s+(""[^""]+?""|[\w.\-/\\]+)\s+(""[^""]+?""|[\w.\-/\\]+)\s*$", RegexOptions.Multiline);
            foreach (Match match in parameterRegex.Matches(source))
            {
                parameters.Add(StripQuotes(match.Groups[1].Value) + "Key", StripQuotes(match.Groups[2].Value));
                parameters.Add(StripQuotes(match.Groups[1].Value) + "Value", StripQuotes(match.Groups[3].Value));
            }
        }

        public bool Compile(PluginResolver pluginResolver)
        {
            // Prepare compilation of C# makefile
            var assemblyLocations = new List<string> {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
                typeof(Command).Assembly.Location, // Add BuildTool.Shared by default
            };

            ParseParameters();

            foreach (string additionalPluginDirectories in parameters.GetRange("PluginDirectory"))
            {
                pluginResolver.AddPluginFolder(Path.Combine(Path.GetDirectoryName(ScriptPath) ?? "", additionalPluginDirectories));
            }

            foreach (string assemblyLocation in parameters.GetRange("Assembly"))
            {
                // 1. Try to find relative to source location first
                var testAssemblyLocation = Path.Combine(Path.GetDirectoryName(ScriptPath) ?? "", assemblyLocation);

                if (!File.Exists(testAssemblyLocation))
                {
                    // 2. Try to find in plugin folders
                    testAssemblyLocation = pluginResolver.FindAssembly(Path.GetFileName(assemblyLocation));

                    if (!File.Exists(testAssemblyLocation))
                    {
                        // 3. Try to find in current assembly directory
                        testAssemblyLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", Path.GetFileName(assemblyLocation) ?? "");

                        if (!File.Exists(testAssemblyLocation))
                            parsingErrors.Add(string.Format("Could not find assembly {0}", assemblyLocation));
                    }
                }
                assemblyLocations.Add(testAssemblyLocation);
            }

            // SourceBaseDirectory - optional
            if (parameters.ContainsSingle("SourceBaseDirectory"))
                SourceBaseDirectory = parameters.GetSingle("SourceBaseDirectory");
            else if (parameters.ContainsMultiple("BuildDirectory"))
                parsingErrors.Add("Source base directory defined multiple times.");
            else
                parsingWarnings.Add("Source base directory not defined.");

            // BuildDirectory - mandatory
            if (parameters.ContainsSingle("BuildDirectory"))
                BuildDirectory = parameters.GetSingle("BuildDirectory");
            else if (parameters.ContainsMultiple("BuildDirectory"))
                parsingErrors.Add("Build directory defined multiple times.");
            else
                parsingErrors.Add("Build directory not defined.");

            // OutputDirectory - optional
            if (parameters.ContainsSingle("OutputDirectory"))
                OutputDirectory = parameters.GetSingle("OutputDirectory");
            else if (parameters.ContainsMultiple("OutputDirectory"))
                parsingErrors.Add("Output directory defined multiple times.");
            else
                parsingWarnings.Add("Output directory not defined.");

            // MetadataDatabaseDirectory - optional
            if (parameters.ContainsSingle("MetadataDatabaseDirectory"))
                MetadataDatabaseDirectory = parameters.GetSingle("MetadataDatabaseDirectory");
            else if (parameters.ContainsMultiple("MetadataDatabaseDirectory"))
                parsingErrors.Add("Metadata database directory defined multiple times.");
            else
                parsingWarnings.Add("Metadata database not defined.");

            var sourceFolderKeys = parameters.GetRange("SourceFolderKey").ToArray();
            var sourceFolderValue = parameters.GetRange("SourceFolderValue").ToArray();

            for (int i = 0; i < sourceFolderKeys.Length; ++i)
            {
                ((Dictionary<string, string>)SourceFolders).Add(sourceFolderKeys[i], sourceFolderValue[i]);
            }

            if (HasErrors)
            {
                return false;
            }

            // Compile C# makefile
            var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
            var compileParams = new CompilerParameters(assemblyLocations.ToArray()) { GenerateInMemory = true, IncludeDebugInformation = true };
            compilerResult = csc.CompileAssemblyFromFile(compileParams, ScriptPath);

            IsCompiled = !HasErrors;
            return IsCompiled;
        }

        public IEnumerable<string> GetErrors()
        {
            IEnumerable<string> errors = parsingErrors;
            if (compilerResult != null)
            {
                errors = errors.Concat(compilerResult.Errors.Cast<CompilerError>().Where(x => !x.IsWarning).Select(x => x.FileName + "(" + x.Line + "): " + x.ErrorText));
            }
            return errors;
        }

        public IEnumerable<string> GetWarnings()
        {
            IEnumerable<string> warnings = parsingWarnings;
            if (compilerResult != null)
            {
                warnings = warnings.Concat(compilerResult.Errors.Cast<CompilerError>().Where(x => x.IsWarning).Select(x => x.FileName + "(" + x.Line + "): " + x.ErrorText));
            }
            return warnings;
        }

        public void Execute(Builder builder)
        {
            // Execute the command C# makefile
            Type type = compilerResult.CompiledAssembly.GetType("BuildScript");
            object makefile = Activator.CreateInstance(type);
            MethodInfo executeMethod = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
            executeMethod.Invoke(makefile, new object[] { builder, builder.Root });
        }

        public void Execute(ListBuildStep root)
        {
            // Execute the command C# makefile
            Type type = compilerResult.CompiledAssembly.GetType("BuildScript");
            if (type != null)
            {
                object makefile = Activator.CreateInstance(type);
                MethodInfo executeMethod = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                executeMethod.Invoke(makefile, new object[] { null, root });
            }
        }
    }
}
