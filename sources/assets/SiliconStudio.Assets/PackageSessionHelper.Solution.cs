// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SiliconStudio.Core;
using SiliconStudio.Core.VisualStudio;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using Version = System.Version;

namespace SiliconStudio.Assets
{
    internal partial class PackageSessionHelper
    {
        private const string SiliconStudioPackage = "SiliconStudioPackage";

        public static bool IsSolutionFile(string filePath)
        {
            return String.Compare(Path.GetExtension(filePath), ".sln", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static Version GetPackageVersion(string fullPath)
        {
            try
            {
                foreach (var packageFullPath in EnumeratePackageFullPaths(fullPath))
                {
                    // Load the package as a Yaml dynamic node, so that we can check Xenko version from dependencies
                    var input = new StringReader(File.ReadAllText(packageFullPath));
                    var yamlStream = new YamlStream();
                    yamlStream.Load(input);
                    dynamic yamlRootNode = new DynamicYamlMapping((YamlMappingNode)yamlStream.Documents[0].RootNode);

                    PackageVersion dependencyVersion = null;

                    foreach (var dependency in yamlRootNode.Meta.Dependencies)
                    {
                        // Support paradox legacy projects
                        if ((string)dependency.Name == "Xenko" || (string)dependency.Name == "Paradox")
                        {
                            dependencyVersion = new PackageVersion((string) dependency.Version);

                            // Paradox 1.1 was having incorrect version set (1.0), read it from .props file
                            if (dependencyVersion.Version.Major == 1 && dependencyVersion.Version.Minor == 0)
                            {
                                var propsFilePath = Path.Combine(Path.GetDirectoryName(packageFullPath) ?? "", Path.GetFileNameWithoutExtension(packageFullPath) + ".props");
                                if (File.Exists(propsFilePath))
                                {
                                    using (XmlReader propsReader = XmlReader.Create(propsFilePath))
                                    {
                                        propsReader.MoveToContent();
                                        if (propsReader.ReadToDescendant("SiliconStudioPackageParadoxVersion"))
                                        {
                                            if (propsReader.Read())
                                            {
                                                dependencyVersion = new PackageVersion(propsReader.Value);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }

                    // Stop after first version
                    if (dependencyVersion != null)
                    {
                        return new Version(dependencyVersion.Version.Major, dependencyVersion.Version.Minor);
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        internal static bool IsPackage(Project project)
        {
            string packagePath;
            return IsPackage(project, out packagePath);
        }

        internal static bool IsPackage(Project project, out string packagePathRelative)
        {
            packagePathRelative = null;
            if (project.IsSolutionFolder && project.Sections.Contains(SiliconStudioPackage))
            {
                var propertyItem = project.Sections[SiliconStudioPackage].Properties.FirstOrDefault();
                if (propertyItem != null)
                {
                    packagePathRelative = propertyItem.Name;
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<string> EnumeratePackageFullPaths(string fullPath)
        {
            if (PackageSessionHelper.IsSolutionFile(fullPath))
            {
                // Solution file: extract projects
                var solutionDirectory = Path.GetDirectoryName(fullPath) ?? "";
                var solution = SiliconStudio.Core.VisualStudio.Solution.FromFile(fullPath);

                foreach (var project in solution.Projects)
                {
                    string packagePath;
                    if (PackageSessionHelper.IsPackage(project, out packagePath))
                    {
                        var packageFullPath = Path.Combine(solutionDirectory, packagePath);
                        yield return packageFullPath;
                    }
                }
            }
            else
            {
                // Otherwise, let's assume it was a package
                yield return fullPath;
            }
        }
    }
}
