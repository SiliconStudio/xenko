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
using System.IO;
using System.Text;

namespace SiliconStudio.Core.VisualStudio
{
    internal class SolutionWriter : IDisposable
    {
        private StreamWriter writer;

        public SolutionWriter(string solutionFullPath) : this(new FileStream(solutionFullPath, FileMode.Create, FileAccess.Write))
        {
        }

        public SolutionWriter(Stream writer)
        {
            this.writer = new StreamWriter(writer, Encoding.UTF8);
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }

        public void Flush()
        {
            writer.Flush();
        }

        public void WriteSolutionFile(Solution solution)
        {
            lock (writer)
            {
                WriteHeader(solution);
                WriteProjects(solution);
                WriteGlobal(solution);
            }
        }

        private void WriteGlobal(Solution solution)
        {
            writer.WriteLine("Global");
            WriteGlobalSections(solution);
            writer.WriteLine("EndGlobal");
        }

        private void WriteGlobalSections(Solution solution)
        {
            foreach (Section globalSection in solution.GlobalSections)
            {
                var propertyLines = new List<PropertyItem>(globalSection.Properties);
                switch (globalSection.Name)
                {
                    case "NestedProjects":
                        foreach (Project project in solution.Projects)
                        {
                            if (project.ParentGuid != Guid.Empty)
                            {
                                propertyLines.Add(new PropertyItem(project.Guid.ToString("B").ToUpperInvariant(), project.ParentGuid.ToString("B").ToUpperInvariant()));
                            }
                        }
                        break;

                    case "ProjectConfigurationPlatforms":
                        foreach (Project project in solution.Projects)
                        {
                            foreach (PropertyItem propertyLine in project.PlatformProperties)
                            {
                                propertyLines.Add(
                                    new PropertyItem(
                                        string.Format("{0}.{1}", project.Guid.ToString("B").ToUpperInvariant(), propertyLine.Name),
                                        propertyLine.Value));
                            }
                        }
                        break;

                    default:
                        if (globalSection.Name.EndsWith("Control", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int index = 1;
                            foreach (Project project in solution.Projects)
                            {
                                if (project.VersionControlProperties.Count > 0)
                                {
                                    foreach (PropertyItem propertyLine in project.VersionControlProperties)
                                    {
                                        propertyLines.Add(
                                            new PropertyItem(
                                                string.Format("{0}{1}", propertyLine.Name, index),
                                                propertyLine.Value));
                                    }
                                    index++;
                                }
                            }

                            propertyLines.Insert(0, new PropertyItem("SccNumberOfProjects", index.ToString()));
                        }
                        break;
                }

                WriteSection(globalSection, propertyLines);
            }
        }

        private void WriteHeader(Solution solution)
        {
            // If the header doesn't start with an empty line, add one
 	        // (The first line of sln files saved as UTF-8 with BOM must be blank, otherwise Visual Studio Version Selector will not detect VS version correctly.)
            if (solution.Headers.Count == 0 || solution.Headers[0].Trim().Length > 0)
 	        {
 	            writer.WriteLine();
 	        }

            foreach (string line in solution.Headers)
            {
                writer.WriteLine(line);
            }

            foreach (PropertyItem propertyLine in solution.Properties)
            {
                writer.WriteLine("{0} = {1}", propertyLine.Name, propertyLine.Value);
            }
        }

        private void WriteProjects(Solution solution)
        {
            foreach (Project project in solution.Projects)
            {
                writer.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    project.TypeGuid.ToString("B").ToUpperInvariant(),
                    project.Name,
                    project.RelativePath,
                    project.Guid.ToString("B").ToUpperInvariant());
                foreach (Section projectSection in project.Sections)
                {
                    WriteSection(projectSection, projectSection.Properties);
                }
                writer.WriteLine("EndProject");
            }
        }

        private void WriteSection(Section section, IEnumerable<PropertyItem> propertyLines)
        {
            writer.WriteLine("\t{0}({1}) = {2}", section.SectionType, section.Name, section.Step);
            foreach (PropertyItem propertyLine in propertyLines)
            {
                writer.WriteLine("\t\t{0} = {1}", propertyLine.Name, propertyLine.Value);
            }
            writer.WriteLine("\tEnd{0}", section.SectionType);
        }
    }
}