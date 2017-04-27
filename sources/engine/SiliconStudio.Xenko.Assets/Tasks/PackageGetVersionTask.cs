// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Assets.Tasks
{
    public class PackageGetVersionTask : Task
    {
        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        [Required]
        public ITaskItem File { get; set; }

        [Output]
        public string NuGetVersion { get; set; }

        [Output]
        public string NugetVersionSimpleNoRevision { get; set; }

        public override bool Execute()
        {
            NuGetVersion = XenkoVersion.NuGetVersion;

            var nugetVersionSimple = new Version(XenkoVersion.NuGetVersionSimple);
            nugetVersionSimple = new Version(nugetVersionSimple.Major, nugetVersionSimple.Minor, nugetVersionSimple.Build);
            NugetVersionSimpleNoRevision = nugetVersionSimple.ToString();
            return true;
        }
    }
}
