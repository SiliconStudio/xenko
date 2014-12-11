// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Manage packages locally installed and accessible on the store.
    /// </summary>
    /// <remarks>
    /// This class is the frontend to the packaging/distribution system. It is currently using nuget for its packaging but may
    /// change in the future.
    /// </remarks>
    public class PackageStore
    {
        private static readonly Lazy<PackageStore> DefaultPackageStore = new Lazy<PackageStore>(() => new PackageStore());

        private const string DefaultEnvironmentSdkDir = "SiliconStudioParadoxDir";

        private const string CommonTargets = @"Targets\SiliconStudio.Common.targets";

        private const string ParadoxSolution = @"build\Paradox.sln";

        private readonly Package defaultPackage;

        private readonly UDirectory globalInstallationPath;

        private readonly UDirectory packagesDirectory;

        private readonly bool isDev;

        private readonly UDirectory defaultPackageDirectory;

        private readonly NugetStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageStore"/> class.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Unable to find a valid Paradox installation path</exception>
        private PackageStore(string installationPath = null, string defaultPackageName = "Paradox", string defaultPackageVersion = ParadoxVersion.CurrentAsText)
        {
            // 1. Try to use the specified installation path
            if (installationPath != null)
            {
                if (!IsRootDirectory(installationPath))
                {
                    throw new ArgumentException("Invalid Paradox installation path [{0}]".ToFormat(installationPath), "installationPath");
                }

                globalInstallationPath = installationPath;
            }

            // TODO: these are currently hardcoded to Paradox
            DefaultPackageName = defaultPackageName;
            DefaultPackageVersion = new PackageVersion(defaultPackageVersion);

            // 2. Try to resolve an installation path from the path of this assembly
            // We need to be able to use the package manager from an official Paradox install as well as from a developer folder

            // Try to determine the root package manager from the current assembly
            var thisAssemblyLocation = typeof(PackageStore).Assembly.Location;
            var binDirectory = !string.IsNullOrWhiteSpace(thisAssemblyLocation) ? new FileInfo(thisAssemblyLocation).Directory : null;
            if (binDirectory != null && binDirectory.Parent != null && binDirectory.Parent.Parent != null)
            {
                var defaultPackageDirectoryTemp = binDirectory.Parent.Parent;

                // If we have a root directory, then store it as the default package directory
                if (IsPackageDirectory(defaultPackageDirectoryTemp.FullName, DefaultPackageName))
                {
                    defaultPackageDirectory = defaultPackageDirectoryTemp.FullName;
                }
                else
                {
                    throw new InvalidOperationException("The current assembly [{0}] is not part of the package [{1}]".ToFormat(thisAssemblyLocation, DefaultPackageName));
                }

                if (globalInstallationPath == null)
                {
                    // Check if we have a regular distribution
                    if (defaultPackageDirectoryTemp.Parent != null && IsRootDirectory(defaultPackageDirectoryTemp.Parent.FullName))
                    {
                        globalInstallationPath = defaultPackageDirectoryTemp.Parent.FullName;
                    }
                    else if (IsRootDirectory(defaultPackageDirectory))
                    {
                        // we have a dev distribution
                        globalInstallationPath = defaultPackageDirectory;
                    }
                }
            }

            // 3. Try from the environement variable
            if (globalInstallationPath == null)
            {
                var rootDirectory = Environment.GetEnvironmentVariable(DefaultEnvironmentSdkDir);
                if (!string.IsNullOrWhiteSpace(rootDirectory) && IsRootDirectory(rootDirectory))
                {
                    globalInstallationPath = rootDirectory;
                    if (defaultPackageDirectory == null)
                    {
                        defaultPackageDirectory = globalInstallationPath;
                    }
                }
            }

            // If there is no root, this is an error
            if (globalInstallationPath == null)
            {
                throw new InvalidOperationException("Unable to find a valid Paradox installation or dev path");
            }

            // Preload default package
            var logger = new LoggerResult();
            var defaultPackageFile = GetPackageFile(defaultPackageDirectory, DefaultPackageName);
            defaultPackage = Package.Load(logger, defaultPackageFile, GetDefaultPackageLoadParameters());
            if (defaultPackage == null)
            {
                throw new InvalidOperationException("Error while loading default package from [{0}]: {1}".ToFormat(defaultPackageFile, logger.ToText()));
            }
            defaultPackage.IsSystem = true;

            // A flag variable just to know if it is a bare bone development directory
            isDev = defaultPackageDirectory != null && IsRootDevDirectory(defaultPackageDirectory);

            // Check if we are in a root directory with store/packages facilities
            if (NugetStore.IsStoreDirectory(globalInstallationPath))
            {
                packagesDirectory = UPath.Combine(globalInstallationPath, (UDirectory)NugetStore.DefaultGamePackagesDirectory);
                store = new NugetStore(globalInstallationPath);
            }
        }

        /// <summary>
        /// Gets or sets the default package name (mainly used in dev environment).
        /// </summary>
        /// <value>The default package name.</value>
        public string DefaultPackageName { get; private set; }

        /// <summary>
        /// Gets the default package minimum version.
        /// </summary>
        /// <value>The default package minimum version.</value>
        public PackageVersionRange DefaultPackageMinVersion
        {
            get
            {
                return new PackageVersionRange(DefaultPackageVersion, true);
            }
        }

        /// <summary>
        /// Gets the default package version.
        /// </summary>
        /// <value>The default package version.</value>
        public PackageVersion DefaultPackageVersion { get; private set; }

        /// <summary>
        /// Gets the default package.
        /// </summary>
        /// <value>The default package.</value>
        public Package DefaultPackage
        {
            get
            {
                return defaultPackage;
            }
        }

        /// <summary>
        /// The root directory of packages.
        /// </summary>
        public UDirectory InstallationPath
        {
            get
            {
                return globalInstallationPath;
            }
        }

        /// <summary>
        /// Gets the packages available online.
        /// </summary>
        /// <returns>IEnumerable&lt;PackageMeta&gt;.</returns>
        public IEnumerable<PackageMeta> GetPackages()
        {
            if (store == null)
            {
                return Enumerable.Empty<PackageMeta>().AsQueryable();
            }

            var packages = store.Manager.SourceRepository.Search(null, false);

            // Order by download count and Id to allow collapsing 
            var orderedPackages = packages.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Id);

            // For some unknown reasons, we can't select directly from IQueryable<IPackage> to IQueryable<PackageMeta>, 
            // so we need to pass through a IEnumerable<PackageMeta> and translate it to IQueyable. Not sure it has
            // an implication on the original query behinds the scene 
            return orderedPackages.Select(PackageMeta.FromNuGet);
        }

        /// <summary>
        /// Gets the packages installed locally.
        /// </summary>
        /// <returns>An enumeratior of <see cref="Package"/>.</returns>
        public IEnumerable<Package> GetInstalledPackages()
        {
            var packages = new List<Package> { defaultPackage };

            if (store != null)
            {
                var log = new LoggerResult();

                var metas = store.Manager.LocalRepository.GetPackages();
                foreach (var meta in metas)
                {
                    var path = store.PathResolver.GetPackageDirectory(meta.Id, meta.Version);

                    var package = Package.Load(log, path, GetDefaultPackageLoadParameters());
                    if (package != null && packages.All(packageRegistered => packageRegistered.Meta.Name != defaultPackage.Meta.Name))
                    {
                        package.IsSystem = true;
                        packages.Add(package);
                    }
                }
            }

            return packages;
        }

        /// <summary>
        /// Gets the filename to the specific package.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="versionRange">The version range.</param>
        /// <param name="allowPreleaseVersion">if set to <c>true</c> [allow prelease version].</param>
        /// <param name="allowUnlisted">if set to <c>true</c> [allow unlisted].</param>
        /// <returns>A location on the disk to the specified package or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">packageName</exception>
        public UFile GetPackageFileName(string packageName, PackageVersionRange versionRange = null, bool allowPreleaseVersion = true, bool allowUnlisted = false)
        {
            if (packageName == null) throw new ArgumentNullException("packageName");
            var directory = GetPackageDirectory(packageName, versionRange, allowPreleaseVersion, allowUnlisted);
            return directory != null ? UPath.Combine(directory, new UFile(packageName + Package.PackageFileExtension)) : null;
        }

        /// <summary>
        /// Gets the default package manager.
        /// </summary>
        /// <value>A default instance.</value>
        public static PackageStore Instance
        {
            get
            {
                return DefaultPackageStore.Value;
            }
        }

        private static PackageLoadParameters GetDefaultPackageLoadParameters()
        {
            // By default, we are not loading assets for installed packages
            return new PackageLoadParameters() { AutoLoadTemporaryAssets = false };
        }

        private UDirectory GetPackageDirectory(string packageName, PackageVersionRange versionRange, bool allowPreleaseVersion = false, bool allowUnlisted = false)
        {
            if (packageName == null) throw new ArgumentNullException("packageName");

            if (store != null)
            {
                var versionSpec = versionRange.ToVersionSpec();
                var package = store.Manager.LocalRepository.FindPackage(packageName, versionSpec, allowPreleaseVersion, allowUnlisted);

                // If package was not found, 
                if (package != null)
                {
                    var directory = store.PathResolver.GetPackageDirectory(package);
                    if (directory != null)
                    {
                        return directory;
                    }
                }
            }

            // TODO: Check version for default package
            return DefaultPackageName == packageName ? defaultPackageDirectory : null;
        }

        private static string GetCommonTargets(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            return Path.Combine(directory, CommonTargets);
        }

        private static string GetPackageFile(string directory, string packageName)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            return Path.Combine(directory, packageName + Package.PackageFileExtension);
        }

        private static bool IsRootDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            var commonTargets = GetCommonTargets(directory);
            return File.Exists(commonTargets);
        }

        private static bool IsPackageDirectory(string directory, string packageName)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            var packageFile = GetPackageFile(directory, packageName);
            return File.Exists(packageFile);
        }

        private static bool IsRootDevDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            var paradoxSolution = Path.Combine(directory, ParadoxSolution);
            return File.Exists(paradoxSolution);
        }
    }
}