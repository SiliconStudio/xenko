// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Mono.Options;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Paradox.FixProjectReferences
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            var isSavingMode = false;
            int exitCode = 0;

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2013 Silicon Studio Corporation. All Rights Reserved",
                    "Paradox Fix Project References - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} [options]* inputSlnFile", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "s|save", "Save mode. By default doesn't save projects", v => isSavingMode = v != null },
                };

            try
            {
                var inputFiles = p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                if (inputFiles.Count != 1)
                    throw new OptionException("Expect only one input file", "");

                var inputFile = inputFiles[0];

                // Read .sln
                var solution = Solution.FromFile(inputFile);

                // Process each project and select the one that will be processed
                foreach (var solutionProject in solution.Projects.ToArray())
                {
                    // Is it really a project?
                    if (!solutionProject.FullPath.EndsWith(".csproj"))
                        continue;

                    // Load XML project
                    var doc = XDocument.Load(solutionProject.FullPath);
                    var ns = doc.Root.Name.Namespace;
                    var allElements = doc.DescendantNodes().OfType<XElement>().ToList();

                    bool hasOutputPath = allElements.Any(element => element.Name.LocalName == "OutputPath");
                    if (!hasOutputPath)
                    {
                        bool projectUpdated = false;
                        //doc.Save(solutionProject.FullPath);
                        Console.WriteLine("Update project [{0}]", solutionProject.FullPath);

                        foreach (var referenceNode in allElements.Where(element => element.Name.LocalName == "ProjectReference"))
                        {
                            var attr = referenceNode.Attribute("Include");
                            if (attr != null && attr.Value.EndsWith("csproj"))
                            {
                                var isPrivate = referenceNode.DescendantNodes().OfType<XElement>().FirstOrDefault(element => element.Name.LocalName == "Private");
                                bool referenceUpdated = false;
                                if (isPrivate == null)
                                {
                                    referenceNode.Add(new XElement(XName.Get("Private", ns.NamespaceName)) {Value = "False"});
                                    referenceUpdated = true;
                                    projectUpdated = true;
                                }
                                else if (!string.IsNullOrEmpty(isPrivate.Value) && string.Compare(isPrivate.Value, "false", true, CultureInfo.InvariantCulture) != 0)
                                {
                                    referenceUpdated = true;
                                    isPrivate.Value = "False";
                                    projectUpdated = true;
                                }
                                if (referenceUpdated)
                                {
                                    Console.WriteLine("    -> Set Private to False [{0}]", attr.Value);
                                }
                            }
                        }

                        if (projectUpdated)
                        {
                            if (isSavingMode)
                            {
                                doc.Save(solutionProject.FullPath);
                                Console.WriteLine("Project Updated [{0}]", solutionProject.Name);
                            }
                            else
                            {
                                Console.WriteLine("Project needs to be updated [{0}]. Run this command with -s switch", solutionProject.Name);
                            }
                        }
                    }
                }
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
