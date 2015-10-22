// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Xml;
using System.Xml.Linq;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Xenko.ProjectGenerator
{
    public class ProjectProcessorContext
    {
        public bool Modified { get; set; }
        public Solution Solution { get; private set; }
        public Project Project { get; private set; }
        public XDocument Document { get; internal set; }
        public XmlNamespaceManager NamespaceManager { get; private set; }

        public ProjectProcessorContext(Solution solution, Project project, XDocument document, XmlNamespaceManager namespaceManager)
        {
            Solution = solution;
            Project = project;
            Document = document;
            NamespaceManager = namespaceManager;
        }
    }
}