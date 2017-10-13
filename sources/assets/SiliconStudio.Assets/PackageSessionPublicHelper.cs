// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Helper class to load/save a VisualStudio solution.
    /// </summary>
    public class PackageSessionPublicHelper
    {
        /// <summary>
        /// This method finds a compatible version of MSBuild and set environment variables so that Microsoft classes can asset it.
        /// It is needed since Microsoft regression introduced in VS15.2. Hopefully this code can be removed in the future.
        /// </summary>
        public static bool FindAndSetMSBuildVersion()
        {
            // Find a compatible version of MSBuild with the required workloads
            var buildTools = VisualStudioVersions.AvailableBuildTools
                .Where(x => x.Version >= new Version("15.0")).OrderByDescending(x => x.Version)
                .FirstOrDefault(x =>
                {
                    // FIXME: factorize with SiliconStudio.Xenko.PackageInstall.Program
                    // FIXME: ideally prompt to install the missing prerequisites
                    if (x.PackageVersions.ContainsKey("Microsoft.VisualStudio.Workload.ManagedDesktop"))
                        return true;
                    if (x.PackageVersions.ContainsKey("Microsoft.VisualStudio.Workload.MSBuildTools") && x.PackageVersions.ContainsKey("Microsoft.Net.Component.4.6.1.TargetingPack"))
                        return true;
                    return false;
                });
            if (buildTools != null)
            {
                Environment.SetEnvironmentVariable("VSINSTALLDIR", buildTools.InstallationPath);
                Environment.SetEnvironmentVariable("VisualStudioVersion", @"15.0");
            }
            // Check that we can create a project
            var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection();
            return projectCollection.GetToolset("15.0") != null;
        }
    }
}
