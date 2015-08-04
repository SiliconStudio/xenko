// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// An analysis to check the validity of a <see cref="Package"/>, convert <see cref="UFile"/> or <see cref="UDirectory"/>
    /// references to absolute/relative paths, check asset references...etc, change <see cref="IContentReference"/> location
    /// if location changed.
    /// </summary>
    public sealed class PackageAnalysis
    {
        private readonly Package package;
        private readonly PackageAnalysisParameters parameters;

        public PackageAnalysis(Package package, PackageAnalysisParameters parameters = null)
        {
            if (package == null) throw new ArgumentNullException("package");
            this.parameters = parameters ?? new PackageAnalysisParameters();
            this.package = package;
        }

        /// <summary>
        /// Gets the parameters used for this analysis.
        /// </summary>
        /// <value>The parameters.</value>
        public PackageAnalysisParameters Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Runs a full analysis on this package.
        /// </summary>
        /// <returns>LoggerResult.</returns>
        public LoggerResult Run()
        {
            var log = new LoggerResult();
            Run(log);
            return log;
        }

        /// <summary>
        /// Runs a full analysis on this package.
        /// </summary>
        /// <param name="log">The log.</param>
        public void Run(ILogger log)
        {
            if (log == null) throw new ArgumentNullException("log");

            // If the package doesn't have a meta name, fix it here
            if (string.IsNullOrWhiteSpace(package.Meta.Name) && package.FullPath != null)
            {
                package.Meta.Name = package.FullPath.GetFileName();
                package.IsDirty = true;
            }

            if (Parameters.IsPackageCheckDependencies)
            {
                CheckDependencies().CopyTo(log);
            }

            if (Parameters.IsProcessingUPaths)
            {
                ProcessPackageUPaths();
            }

            ProcessAssets().CopyTo(log);
        }

        /// <summary>
        /// Checks the package.
        /// </summary>
        /// <returns>LoggerResult.</returns>
        public LoggerResult CheckDependencies()
        {
            var log = new LoggerResult();

            // Can only check dependencies if we are inside a session
            if (package.Session == null)
            {
                return log;
            }

            // If ProjetcPath is null, the package was not saved.
            if (Parameters.ConvertUPathTo == UPathType.Relative && package.FullPath == null)
            {
                log.Error(package, null, AssetMessageCode.PackageFilePathNotSet);
                return log;
            }

            // 1. Check all store package references
            foreach (var packageDependency in package.Meta.Dependencies)
            {
                // Try to find the package reference
                var subPackage = package.Session.Packages.Find(packageDependency);
                if (subPackage == null)
                {
                    // Originally we were fixing DefaultPackage version, but it should now be handled by package upgraders
                    log.Error(package, null, AssetMessageCode.PackageNotFound, packageDependency);
                }
            }

            // 2. Check all local package references
            foreach (var packageReference in package.LocalDependencies)
            {
                // Try to find the package reference
                var newSubPackage = package.Session.Packages.Find(packageReference.Id);
                if (newSubPackage == null)
                {
                    log.Error(package, null, AssetMessageCode.PackageNotFound, packageReference.Location);
                    continue;
                }

                if (newSubPackage.FullPath == null || newSubPackage.IsSystem)
                {
                    continue;
                }

                // If package was found, check that the path is correctly setup
                var pathToSubPackage = Parameters.ConvertUPathTo == UPathType.Relative ? newSubPackage.FullPath.MakeRelative(package.RootDirectory) : newSubPackage.FullPath;
                if (packageReference.Location != pathToSubPackage)
                {
                    // Modify package path to be relative if different
                    packageReference.Location = pathToSubPackage;

                    if (Parameters.SetDirtyFlagOnAssetWhenFixingUFile)
                    {
                        package.IsDirty = true;
                    }
                }
            }

            // TODO: Check profiles

            return log;
        }

        /// <summary>
        /// Processes the UPaths on package (but not on assets, use <see cref="ProcessAssets"/> for this)
        /// </summary>
        public void ProcessPackageUPaths()
        {
            if (package.FullPath == null)
            {
                return;
            }

            var packageReferenceLinks = AssetReferenceAnalysis.Visit(package);
            CommonAnalysis.UpdatePaths(package, packageReferenceLinks.Where(link => link.Reference is UPath), Parameters);
        }

        public LoggerResult ProcessAssets()
        {
            var log = new LoggerResult();

            var assets = (package.TemporaryAssets.Count > 0 ? (IEnumerable<AssetItem>)package.TemporaryAssets : package.Assets);

            foreach (var assetItem in assets)
            {
                AssetAnalysis.Run(assetItem, log, Parameters);
            }

            return log;
        }
    }
}