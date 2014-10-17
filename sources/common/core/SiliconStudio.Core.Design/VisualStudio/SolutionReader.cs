#region License

// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SiliconStudio.Core.VisualStudio
{
    internal class SolutionReader : IDisposable
    {
        private static readonly Regex RegexConvertEscapedValues = new Regex(@"\\u(?<HEXACODE>[0-9a-fA-F]{4})");
        private static readonly Regex RegexParseGlobalSection = new Regex(@"^(?<TYPE>GlobalSection)\((?<NAME>.*)\) = (?<STEP>.*)$");
        private static readonly Regex RegexParseProject = new Regex("^Project\\(\"(?<PROJECTTYPEGUID>.*)\"\\)\\s*=\\s*\"(?<PROJECTNAME>.*)\"\\s*,\\s*\"(?<RELATIVEPATH>.*)\"\\s*,\\s*\"(?<PROJECTGUID>.*)\"$");
        private static readonly Regex RegexParseProjectConfigurationPlatformsName = new Regex(@"^(?<GUID>\{[-0-9a-zA-Z]+\})\.(?<DESCRIPTION>.*)$");
        private static readonly Regex RegexParseProjectSection = new Regex(@"^(?<TYPE>ProjectSection)\((?<NAME>.*)\) = (?<STEP>.*)$");
        private static readonly Regex RegexParsePropertyLine = new Regex(@"^(?<PROPERTYNAME>[^=]*)\s*=\s*(?<PROPERTYVALUE>[^=]*)$");
        private static readonly Regex RegexParseVersionControlName = new Regex(@"^(?<NAME_WITHOUT_INDEX>[a-zA-Z]*)(?<INDEX>[0-9]+)$");
        private Solution solution;
        private int currentLineNumber;
        private StreamReader reader;

        public SolutionReader(string solutionFullPath) : this(new FileStream(solutionFullPath, FileMode.Open, FileAccess.Read))
        {
        }

        public SolutionReader(Stream reader)
        {
            this.reader = new StreamReader(reader, Encoding.Default);
            currentLineNumber = 0;
        }

        public void Dispose()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }

        public Solution ReadSolutionFile()
        {
            lock (reader)
            {
                solution = new Solution();
                ReadHeader();
                for (string line = ReadLine(); line != null; line = ReadLine())
                {
                    // Skip blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("Project(", StringComparison.InvariantCultureIgnoreCase))
                    {
                        solution.Projects.Add(ReadProject(line));
                    }
                    else if (String.Compare(line, "Global", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        ReadGlobal();
                        // TODO valide end of file
                        break;
                    }
                    else if (RegexParsePropertyLine.Match(line).Success)
                    {
                        // Read VS properties (introduced in VS2012/VS2013?)
                        solution.Properties.Add(ReadPropertyLine(line));
                    } else 
                    {
                        throw new SolutionFileException(string.Format("Invalid line read on line #{0}.\nFound: {1}\nExpected: A line beginning with 'Project(' or 'Global'.",
                            currentLineNumber,
                            line));
                    }
                }
                return solution;
            }
        }

        private Project FindProjectByGuid(string guid, int lineNumber)
        {
            Project p = solution.Projects.FindByGuid(new Guid(guid));
            if (p == null)
            {
                throw new SolutionFileException(string.Format("Invalid guid found on line #{0}.\nFound: {1}\nExpected: A guid from one of the projects in the solution.",
                    lineNumber,
                    guid));
            }
            return p;
        }

        private void HandleNestedProjects(string name, string type, string step, List<PropertyItem> propertyLines, int startLineNumber)
        {
            int localLineNumber = startLineNumber;
            foreach (PropertyItem propertyLine in propertyLines)
            {
                localLineNumber++;
                Project left = FindProjectByGuid(propertyLine.Name, localLineNumber);
                left.ParentGuid = new Guid(propertyLine.Value);
            }
            solution.GlobalSections.Add(
                new Section(
                    name,
                    type,
                    step,
                    null));
        }

        private void HandleProjectConfigurationPlatforms(string name, string type, string step, List<PropertyItem> propertyLines, int startLineNumber)
        {
            int localLineNumber = startLineNumber;
            foreach (PropertyItem propertyLine in propertyLines)
            {
                localLineNumber++;
                Match match = RegexParseProjectConfigurationPlatformsName.Match(propertyLine.Name);
                if (! match.Success)
                {
                    throw new SolutionFileException(string.Format("Invalid format for a project configuration name on line #{0}.\nFound: {1}",
                        currentLineNumber,
                        propertyLine.Name
                        ));
                }

                string projectGuid = match.Groups["GUID"].Value;
                string description = match.Groups["DESCRIPTION"].Value;
                Project left = FindProjectByGuid(projectGuid, localLineNumber);
                left.PlatformProperties.Add(
                    new PropertyItem(
                        description,
                        propertyLine.Value));
            }
            solution.GlobalSections.Add(
                new Section(
                    name,
                    type,
                    step,
                    null));
        }

        private void HandleVersionControlLines(string name, string type, string step, List<PropertyItem> propertyLines)
        {
            var propertyLinesByIndex = new Dictionary<int, List<PropertyItem>>();
            var othersVersionControlLines = new List<PropertyItem>();
            foreach (PropertyItem propertyLine in propertyLines)
            {
                Match match = RegexParseVersionControlName.Match(propertyLine.Name);
                if (match.Success)
                {
                    string nameWithoutIndex = match.Groups["NAME_WITHOUT_INDEX"].Value.Trim();
                    int index = int.Parse(match.Groups["INDEX"].Value.Trim());

                    if (!propertyLinesByIndex.ContainsKey(index))
                    {
                        propertyLinesByIndex[index] = new List<PropertyItem>();
                    }
                    propertyLinesByIndex[index].Add(new PropertyItem(nameWithoutIndex, propertyLine.Value));
                }
                else
                {
                    // Ignore SccNumberOfProjects. This number will be computed and added by the SolutionFileWriter class.
                    if (propertyLine.Name != "SccNumberOfProjects")
                    {
                        othersVersionControlLines.Add(propertyLine);
                    }
                }
            }

            // Handle the special case for the solution itself.
            othersVersionControlLines.Add(new PropertyItem("SccLocalPath0", "."));

            foreach (var item in propertyLinesByIndex)
            {
                int index = item.Key;
                List<PropertyItem> propertiesForIndex = item.Value;

                PropertyItem uniqueNameProperty = propertiesForIndex.Find(delegate(PropertyItem property) { return property.Name == "SccProjectUniqueName"; });
                // If there is no ProjectUniqueName, we assume that it's the entry related to the solution by itself. We
                // can ignore it because we added the special case above.
                if (uniqueNameProperty != null)
                {
                    string uniqueName = RegexConvertEscapedValues.Replace(uniqueNameProperty.Value, delegate(Match match)
                    {
                        int hexaValue = int.Parse(match.Groups["HEXACODE"].Value, NumberStyles.AllowHexSpecifier);
                        return char.ConvertFromUtf32(hexaValue);
                    });
                    uniqueName = uniqueName.Replace(@"\\", @"\");

                    Project relatedProject = null;
                    foreach (Project project in solution.Projects)
                    {
                        if (string.Compare(project.RelativePath, uniqueName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            relatedProject = project;
                        }
                    }
                    if (relatedProject == null)
                    {
                        throw new SolutionFileException(
                            string.Format("Invalid value for the property 'SccProjectUniqueName{0}' of the global section '{1}'.\nFound: {2}\nExpected: A value equal to the field 'RelativePath' of one of the projects in the solution.",
                                index,
                                name,
                                uniqueName));
                    }

                    relatedProject.VersionControlProperties.AddRange(propertiesForIndex);
                }
            }

            solution.GlobalSections.Add(
                new Section(
                    name,
                    type,
                    step,
                    othersVersionControlLines));
        }

        private void ReadGlobal()
        {
            for (string line = ReadLine(); !line.StartsWith("EndGlobal"); line = ReadLine())
            {
                ReadGlobalSection(line);
            }
        }

        private void ReadGlobalSection(string firstLine)
        {
            Match match = RegexParseGlobalSection.Match(firstLine);
            if (! match.Success)
            {
                throw new SolutionFileException(string.Format("Invalid format for a global section on line #{0}.\nFound: {1}",
                    currentLineNumber,
                    firstLine
                    ));
            }

            string type = match.Groups["TYPE"].Value.Trim();
            string name = match.Groups["NAME"].Value.Trim();
            string step = match.Groups["STEP"].Value.Trim();

            var propertyLines = new List<PropertyItem>();
            int startLineNumber = currentLineNumber;
            string endOfSectionToken = "End" + type;
            for (string line = ReadLine(); !line.StartsWith(endOfSectionToken, StringComparison.InvariantCultureIgnoreCase); line = ReadLine())
            {
                propertyLines.Add(ReadPropertyLine(line));
            }

            switch (name)
            {
                case "NestedProjects":
                    HandleNestedProjects(name, type, step, propertyLines, startLineNumber);
                    break;

                case "ProjectConfigurationPlatforms":
                    HandleProjectConfigurationPlatforms(name, type, step, propertyLines, startLineNumber);
                    break;

                default:
                    if (name.EndsWith("Control", StringComparison.InvariantCultureIgnoreCase))
                    {
                        HandleVersionControlLines(name, type, step, propertyLines);
                    }
                    else
                    {
                        solution.GlobalSections.Add(
                            new Section(
                                name,
                                type,
                                step,
                                propertyLines));
                    }
                    break;
            }
        }

        private void ReadHeader()
        {
            for (int i = 1; i <= 3; i++)
            {
                string line = ReadLine();
                solution.Headers.Add(line);
                if (line.StartsWith("#"))
                {
                    return;
                }
            }
        }

        private string ReadLine()
        {
            string line = reader.ReadLine();
            if (line == null)
            {
                throw new SolutionFileException("Unexpected end of file encounted while reading the solution file.");
            }

            currentLineNumber++;
            return line.Trim();
        }

        private Project ReadProject(string firstLine)
        {
            Match match = RegexParseProject.Match(firstLine);
            if (!match.Success)
            {
                throw new SolutionFileException(string.Format("Invalid format for a project on line #{0}.\nFound: {1}.",
                    currentLineNumber,
                    firstLine
                    ));
            }

            string projectTypeGuid = match.Groups["PROJECTTYPEGUID"].Value.Trim();
            string projectName = match.Groups["PROJECTNAME"].Value.Trim();
            string relativePath = match.Groups["RELATIVEPATH"].Value.Trim();
            string projectGuid = match.Groups["PROJECTGUID"].Value.Trim();

            var projectSections = new List<Section>();
            for (string line = ReadLine(); !line.StartsWith("EndProject"); line = ReadLine())
            {
                projectSections.Add(ReadProjectSection(line));
            }

            return new Project(
                solution,
                new Guid(projectGuid),
                new Guid(projectTypeGuid),
                projectName,
                relativePath,
                Guid.Empty,
                projectSections,
                null,
                null);
        }

        private Section ReadProjectSection(string firstLine)
        {
            Match match = RegexParseProjectSection.Match(firstLine);
            if (!match.Success)
            {
                throw new SolutionFileException(string.Format("Invalid format for a project section on line #{0}.\nFound: {1}.",
                    currentLineNumber,
                    firstLine
                    ));
            }

            string type = match.Groups["TYPE"].Value.Trim();
            string name = match.Groups["NAME"].Value.Trim();
            string step = match.Groups["STEP"].Value.Trim();

            var propertyLines = new List<PropertyItem>();
            string endOfSectionToken = "End" + type;
            for (string line = ReadLine(); !line.StartsWith(endOfSectionToken, StringComparison.InvariantCultureIgnoreCase); line = ReadLine())
            {
                propertyLines.Add(ReadPropertyLine(line));
            }
            return new Section(name, type, step, propertyLines);
        }

        private PropertyItem ReadPropertyLine(string line)
        {
            Match match = RegexParsePropertyLine.Match(line);
            if (!match.Success)
            {
                throw new SolutionFileException(string.Format("Invalid format for a property on line #{0}.\nFound: {1}.",
                    currentLineNumber,
                    line
                    ));
            }

            return new PropertyItem(
                match.Groups["PROPERTYNAME"].Value.Trim(),
                match.Groups["PROPERTYVALUE"].Value.Trim());
        }
    }
}