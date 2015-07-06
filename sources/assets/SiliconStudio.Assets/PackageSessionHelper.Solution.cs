// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Assets
{
    internal partial class PackageSessionHelper
    {
        private const string SiliconStudioPackage = "SiliconStudioPackage";

        public static bool IsSolutionFile(string filePath)
        {
            return String.Compare(Path.GetExtension(filePath), ".sln", StringComparison.InvariantCultureIgnoreCase) == 0;
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
    }
}