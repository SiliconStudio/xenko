// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
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
            // TODO: these are currently hardcoded to Paradox
            DefaultPackageName = defaultPackageName;
            DefaultPackageVersion = new PackageVersion(defaultPackageVersion);
            defaultPackageDirectory = DirectoryHelper.GetPackageDirectory(defaultPackageName);
   
            // 1. Try to use the specified installation path
            if (installationPath != null)
            {
                if (!DirectoryHelper.IsInstallationDirectory(installationPath))
                {
                    throw new ArgumentException("Invalid Paradox installation path [{0}]".ToFormat(installationPath), "installationPath");
                }

                globalInstallationPath = installationPath;
            }

            // 2. Try to resolve an installation path from the path of this assembly
            // We need to be able to use the package manager from an official Paradox install as well as from a developer folder
            if (globalInstallationPath == null)
            {
                globalInstallationPath = DirectoryHelper.GetInstallationDirectory(DefaultPackageName);
            }

            // If there is no root, this is an error
            if (globalInstallationPath == null)
            {
                throw new InvalidOperationException("Unable to find a valid Paradox installation or dev path");
            }

            // Preload default package
            var logger = new LoggerResult();
            var defaultPackageFile = DirectoryHelper.GetPackageFile(defaultPackageDirectory, DefaultPackageName);
            defaultPackage = Package.Load(logger, defaultPackageFile, GetDefaultPackageLoadParameters());
            if (defaultPackage == null)
            {
                throw new InvalidOperationException("Error while loading default package from [{0}]: {1}".ToFormat(defaultPackageFile, logger.ToText()));
            }
            defaultPackage.IsSystem = true;

            // A flag variable just to know if it is a bare bone development directory
            isDev = defaultPackageDirectory != null && DirectoryHelper.IsRootDevDirectory(defaultPackageDirectory);

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
            return directory != null ? UPath.Combine(UPath.Combine(UPath.Combine(InstallationPath, (UDirectory)store.RepositoryPath), directory), new UFile(packageName + Package.PackageFileExtension)) : null;
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
            return new PackageLoadParameters { AutoLoadTemporaryAssets = false };
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
    }
}