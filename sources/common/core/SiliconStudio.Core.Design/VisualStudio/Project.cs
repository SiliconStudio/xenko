#region License
// Copyright (c) 2012-2014 Silicon Studio Corporation (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
// SLNTools
// Copyright (c) 2009 
// by Christian Warren
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace SiliconStudio.Core.VisualStudio
{
    /// <summary>
    /// A project referenced by a VisualStudio solution.
    /// </summary>
    public class Project
    {
        private readonly Guid guid;
        private readonly PropertyItemCollection platformProperties;
        private readonly SectionCollection sections;
        private readonly Solution solution;
        private readonly PropertyItemCollection versionControlProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="original">The original.</param>
        public Project(Solution solution, Project original)
            : this(
                solution,
                original.Guid,
                original.TypeGuid,
                original.Name,
                original.RelativePath,
                original.ParentGuid,
                original.Sections,
                original.VersionControlProperties,
                original.PlatformProperties)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="typeGuid">The type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="parentGuid">The parent unique identifier.</param>
        /// <param name="projectSections">The project sections.</param>
        /// <param name="versionControlLines">The version control lines.</param>
        /// <param name="projectConfigurationPlatformsLines">The project configuration platforms lines.</param>
        /// <exception cref="System.ArgumentNullException">
        /// solution
        /// or
        /// guid
        /// or
        /// typeGuid
        /// or
        /// name
        /// </exception>
        public Project(
            Solution solution,
            Guid guid,
            Guid typeGuid,
            string name,
            string relativePath,
            Guid parentGuid,
            IEnumerable<Section> projectSections,
            IEnumerable<PropertyItem> versionControlLines,
            IEnumerable<PropertyItem> projectConfigurationPlatformsLines)
        {
            if (solution == null) throw new ArgumentNullException("solution");
            if (guid == null) throw new ArgumentNullException("guid");
            if (typeGuid == null) throw new ArgumentNullException("typeGuid");
            if (name == null) throw new ArgumentNullException("name");

            this.solution = solution;
            this.guid = guid;
            this.TypeGuid = typeGuid;
            this.Name = name;
            this.RelativePath = relativePath;
            this.ParentGuid = parentGuid;
            sections = new SectionCollection(projectSections);
            versionControlProperties = new PropertyItemCollection(versionControlLines);
            platformProperties = new PropertyItemCollection(projectConfigurationPlatformsLines);
        }

        /// <summary>
        /// Gets all descendants <see cref="Project"/>
        /// </summary>
        /// <value>All descendants.</value>
        public IEnumerable<Project> AllDescendants
        {
            get
            {
                foreach (var child in Children)
                {
                    yield return child;
                    foreach (var subchild in child.AllDescendants)
                    {
                        yield return subchild;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is solution folder.
        /// </summary>
        /// <value><c>true</c> if this instance is solution folder; otherwise, <c>false</c>.</value>
        public bool IsSolutionFolder
        {
            get
            {
                return TypeGuid == KnownProjectTypeGuid.SolutionFolder;
            }
        }

        /// <summary>
        /// Gets all direct child <see cref="Project"/> 
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<Project> Children
        {
            get
            {
                if (IsSolutionFolder)
                {
                    foreach (var project in solution.Projects)
                    {
                        if (project.ParentGuid == guid)
                            yield return project;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all project dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        /// <exception cref="SolutionFileException">
        /// </exception>
        public IEnumerable<Project> Dependencies
        {
            get
            {
                if (ParentProject != null)
                {
                    yield return ParentProject;
                }

                if (sections.Contains("ProjectDependencies"))
                {
                    foreach (PropertyItem propertyLine in sections["ProjectDependencies"].Properties)
                    {
                        string dependencyGuid = propertyLine.Name;
                        yield return FindProjectInContainer(
                            dependencyGuid,
                            "Cannot find one of the dependency of project '{0}'.\nProject guid: {1}\nDependency guid: {2}\nReference found in: ProjectDependencies section of the solution file",
                            Name,
                            guid,
                            dependencyGuid);
                    }
                }


                if (TypeGuid == KnownProjectTypeGuid.VisualC)
                {
                    if (!File.Exists(FullPath))
                    {
                        throw new SolutionFileException(string.Format(
                            "Cannot detect dependencies of projet '{0}' because the project file cannot be found.\nProject full path: '{1}'",
                            Name,
                            FullPath));
                    }

                    var docVisualC = new XmlDocument();
                    docVisualC.Load(FullPath);

                    foreach (XmlNode xmlNode in docVisualC.SelectNodes(@"//ProjectReference"))
                    {
                        string dependencyGuid = xmlNode.Attributes["ReferencedProjectIdentifier"].Value; // TODO handle null
                        string dependencyRelativePathToProject;
                        XmlNode relativePathToProjectNode = xmlNode.Attributes["RelativePathToProject"];
                        if (relativePathToProjectNode != null)
                        {
                            dependencyRelativePathToProject = relativePathToProjectNode.Value;
                        }
                        else
                        {
                            dependencyRelativePathToProject = "???";
                        }
                        yield return FindProjectInContainer(
                            dependencyGuid,
                            "Cannot find one of the dependency of project '{0}'.\nProject guid: {1}\nDependency guid: {2}\nDependency relative path: '{3}'\nReference found in: ProjectReference node of file '{4}'",
                            Name,
                            guid,
                            dependencyGuid,
                            dependencyRelativePathToProject,
                            FullPath);
                    }
                }

                else if (TypeGuid == KnownProjectTypeGuid.Setup)
                {
                    if (!File.Exists(FullPath))
                    {
                        throw new SolutionFileException(string.Format(
                            "Cannot detect dependencies of projet '{0}' because the project file cannot be found.\nProject full path: '{1}'",
                            Name,
                            FullPath));
                    }

                    foreach (string line in File.ReadAllLines(FullPath))
                    {
                        var regex = new Regex("^\\s*\"OutputProjectGuid\" = \"\\d*\\:(?<PROJECTGUID>.*)\"$");
                        Match match = regex.Match(line);
                        if (match.Success)
                        {
                            string dependencyGuid = match.Groups["PROJECTGUID"].Value.Trim();
                            yield return FindProjectInContainer(
                                dependencyGuid,
                                "Cannot find one of the dependency of project '{0}'.\nProject guid: {1}\nDependency guid: {2}\nReference found in: OutputProjectGuid line of file '{3}'",
                                Name,
                                guid,
                                dependencyGuid,
                                FullPath);
                        }
                    }
                }

                else if (TypeGuid == KnownProjectTypeGuid.WebProject)
                {

                    // Format is: "({GUID}|ProjectName;)*"
                    // Example: "{GUID}|Infra.dll;{GUID2}|Services.dll;"
                    PropertyItem propertyItem = sections["WebsiteProperties"].Properties["ProjectReferences"];
                    string value = propertyItem.Value;
                    if (value.StartsWith("\""))
                        value = value.Substring(1);
                    if (value.EndsWith("\""))
                        value = value.Substring(0, value.Length - 1);

                    foreach (string dependency in value.Split(';'))
                    {
                        if (dependency.Trim().Length > 0)
                        {
                            string[] parts = dependency.Split('|');
                            string dependencyGuid = parts[0];
                            string dependencyName = parts[1]; // TODO handle null
                            yield return FindProjectInContainer(
                                dependencyGuid,
                                "Cannot find one of the dependency of project '{0}'.\nProject guid: {1}\nDependency guid: {2}\nDependency name: {3}\nReference found in: ProjectReferences line in WebsiteProperties section of the solution file",
                                Name,
                                guid,
                                dependencyGuid,
                                dependencyName);
                        }
                    }
                }
                else if (!IsSolutionFolder)
                {
                    if (!File.Exists(FullPath))
                    {
                        throw new SolutionFileException(string.Format(
                            "Cannot detect dependencies of projet '{0}' because the project file cannot be found.\nProject full path: '{1}'",
                            Name,
                            FullPath));
                    }

                    var docManaged = new XmlDocument();
                    docManaged.Load(FullPath);

                    var xmlManager = new XmlNamespaceManager(docManaged.NameTable);
                    xmlManager.AddNamespace("prefix", "http://schemas.microsoft.com/developer/msbuild/2003");

                    foreach (XmlNode xmlNode in docManaged.SelectNodes(@"//prefix:ProjectReference", xmlManager))
                    {
                        string dependencyGuid = xmlNode.SelectSingleNode(@"prefix:Project", xmlManager).InnerText.Trim(); // TODO handle null
                        string dependencyName = xmlNode.SelectSingleNode(@"prefix:Name", xmlManager).InnerText.Trim(); // TODO handle null
                        yield return FindProjectInContainer(
                            dependencyGuid,
                            "Cannot find one of the dependency of project '{0}'.\nProject guid: {1}\nDependency guid: {2}\nDependency name: {3}\nReference found in: ProjectReference node of file '{4}'",
                            Name,
                            guid,
                            dependencyGuid,
                            dependencyName,
                            FullPath);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>The solution.</value>
        public Solution Solution
        {
            get
            {
                return solution;
            }
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(solution.FullPath), RelativePath);
            }
        }

        /// <summary>
        /// Gets the parent project.
        /// </summary>
        /// <value>The parent project.</value>
        public Project ParentProject
        {
            get
            {
                if (ParentGuid == Guid.Empty)
                    return null;

                return FindProjectInContainer(
                    ParentGuid,
                    "Cannot find the parent folder of project '{0}'. \nProject guid: {1}\nParent folder guid: {2}",
                    Name,
                    guid,
                    ParentGuid);
            }
        }

        /// <summary>
        /// Gets or sets the parent folder unique identifier.
        /// </summary>
        /// <value>The parent folder unique identifier.</value>
        public Guid ParentGuid { get; set; }

        /// <summary>
        /// Gets the full name of the project.
        /// </summary>
        /// <value>The full name of the project.</value>
        public string FullName
        {
            get
            {
                if (ParentProject != null)
                {
                    return ParentProject.FullName + @"\" + Name;
                }
                return Name;
            }
        }

        /// <summary>
        /// Gets the project unique identifier.
        /// </summary>
        /// <value>The project unique identifier.</value>
        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the project platform properties.
        /// </summary>
        /// <value>The project platform properties.</value>
        public PropertyItemCollection PlatformProperties
        {
            get
            {
                return platformProperties;
            }
        }

        /// <summary>
        /// Gets the project sections.
        /// </summary>
        /// <value>The project sections.</value>
        public SectionCollection Sections
        {
            get
            {
                return sections;
            }
        }

        /// <summary>
        /// Gets or sets the type unique identifier.
        /// </summary>
        /// <value>The type unique identifier.</value>
        public Guid TypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the relative path.
        /// </summary>
        /// <value>The relative path.</value>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets the version control properties.
        /// </summary>
        /// <value>The version control properties.</value>
        public PropertyItemCollection VersionControlProperties
        {
            get
            {
                return versionControlProperties;
            }
        }

        public override string ToString()
        {
            return string.Format("Project '{0}'", FullName);
        }

        private Project FindProjectInContainer(Guid projectGuidToFind, string errorMessageFormat, params object[] errorMessageParams)
        {
            Project project = solution.Projects.FindByGuid(projectGuidToFind);
            if (project == null)
            {
                throw new SolutionFileException(string.Format(errorMessageFormat, errorMessageParams));
            }
            return project;
        }

        private Project FindProjectInContainer(string projectGuidToFind, string errorMessageFormat, params object[] errorMessageParams)
        {
            return FindProjectInContainer(Guid.Parse(projectGuidToFind), errorMessageFormat, errorMessageParams);
        }
    }
}