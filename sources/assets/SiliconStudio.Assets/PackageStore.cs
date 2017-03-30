// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using System.Threading.Tasks;
using SiliconStudio.Packages;

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

        private readonly UDirectory defaultPackageDirectory;

        /// <summary>
        /// Associated NugetStore for our packages. Cannot be null.
        /// </summary>
        private readonly NugetStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageStore"/> class.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Unable to find a valid Xenko installation path</exception>
        private PackageStore(string installationPath = null, string defaultPackageName = "Xenko", string defaultPackageVersion = XenkoVersion.NuGetVersionSimple)
        {
            // TODO: these are currently hardcoded to Xenko
            DefaultPackageName = defaultPackageName;
            DefaultPackageVersion = new PackageVersion(defaultPackageVersion);
            defaultPackageDirectory = DirectoryHelper.GetPackageDirectory(defaultPackageName);
   
            // 1. Try to use the specified installation path
            if (installationPath != null)
            {
                if (!DirectoryHelper.IsInstallationDirectory(installationPath))
                {
                    throw new ArgumentException("Invalid Xenko installation path [{0}]".ToFormat(installationPath), "installationPath");
                }

                globalInstallationPath = installationPath;
            }

            // 2. Try to resolve an installation path from the path of this assembly
            // We need to be able to use the package manager from an official Xenko install as well as from a developer folder
            if (globalInstallationPath == null)
            {
                globalInstallationPath = DirectoryHelper.GetInstallationDirectory(DefaultPackageName);
            }

            // If there is no root, this is an error
            if (globalInstallationPath == null)
            {
                throw new InvalidOperationException("Unable to find a valid Xenko installation or dev path");
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

            // Check if we are in a root directory with store/packages facilities
            if (NugetStore.IsStoreDirectory(globalInstallationPath))
            {
                store = new NugetStore(globalInstallationPath);
            }
            else
            {
                // We should exit from here if NuGet is not configured.
                MessageBox.Show($"Unexpected installation. Cannot find a proper NuGet configuration for [{defaultPackageName}] in [{globalInstallationPath}]", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
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
        public PackageVersion DefaultPackageVersion { get; }

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
        public async Task<IEnumerable<PackageMeta>> GetPackages()
        {
            var packages = await store.SourceSearch(null, allowPrereleaseVersions: false);

            // Order by download count and Id to allow collapsing 
            var orderedPackages = packages.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Id);

            // For some unknown reasons, we can't select directly from IQueryable<IPackage> to IQueryable<PackageMeta>, 
            // so we need to pass through a IEnumerable<PackageMeta> and translate it to IQueyable. Not sure it has
            // an implication on the original query behinds the scene 
            return orderedPackages.Select(PackageMetaFromNugetPackage);
        }

        /// <summary>
        /// Gets the packages installed locally.
        /// </summary>
        /// <returns>An enumeratior of <see cref="Package"/>.</returns>
        public IEnumerable<Package> GetInstalledPackages()
        {
            var packages = new List<Package> { defaultPackage };

            var log = new LoggerResult();

            var metas = store.GetLocalPackages();
            foreach (var meta in metas)
            {
                var path = store.GetPackageDirectory(meta);

                var package = Package.Load(log, path, GetDefaultPackageLoadParameters());
                if (package != null && packages.All(packageRegistered => packageRegistered.Meta.Name != defaultPackage.Meta.Name))
                {
                    package.IsSystem = true;
                    packages.Add(package);
                }
            }

            return packages;
        }

        /// <summary>
        /// Gets the filename to the specific package using just a package name.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <returns>A location on the disk to the specified package or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">packageName</exception>
        public UFile GetPackageWithFileName(string packageName)
        {
            return GetPackageFileName(packageName);
        }

        /// <summary>
        /// Gets the filename to the specific package <paramref name="packageName"/> using the version <paramref name="versionRange"/> if not null, otherwise the <paramref name="constraintProvider"/> if specified.
        /// If no constraints are specified, the first entry if any are founds is used to get the filename.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="versionRange">The version range.</param>
        /// <param name="constraintProvider">The package constraint provider.</param>
        /// <param name="allowPreleaseVersion">if set to <c>true</c> [allow prelease version].</param>
        /// <param name="allowUnlisted">if set to <c>true</c> [allow unlisted].</param>
        /// <returns>A location on the disk to the specified package or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">packageName</exception>
        public UFile GetPackageFileName(string packageName, PackageVersionRange versionRange = null, ConstraintProvider constraintProvider = null, bool allowPreleaseVersion = true, bool allowUnlisted = false)
        {
            if (packageName == null) throw new ArgumentNullException(nameof(packageName));

            var package = store.FindLocalPackage(packageName, versionRange, constraintProvider, allowPreleaseVersion, allowUnlisted);

            // If package was not found, 
            if (package != null)
            {
                return UPath.Combine(store.GetInstallPath(package), new UFile(packageName + Package.PackageFileExtension));
            }

            // TODO: Check version for default package
            if (packageName == DefaultPackageName)
            {
                if (versionRange == null || versionRange.Contains(DefaultPackageVersion))
                {
                    return UPath.Combine(UPath.Combine(UPath.Combine(InstallationPath, (UDirectory)store.RepositoryPath), defaultPackageDirectory), new UFile(packageName + Package.PackageFileExtension));
                }
            }

            return null;
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
            return new PackageLoadParameters { AutoLoadTemporaryAssets = false, LoadAssemblyReferences = false, AutoCompileProjects = false };
        }

        /// <summary>
        /// Is current store a bare bone development one?
        /// </summary>
        private bool IsDevelopmentStore => defaultPackageDirectory != null && DirectoryHelper.IsRootDevDirectory(defaultPackageDirectory);

        /// <summary>
        /// New instance of <see cref="PackageMeta"/> from a nuget package <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The nuget metadata used to initialized an instance of <see cref="PackageMeta"/>.</param>
        public static PackageMeta PackageMetaFromNugetPackage(NugetPackage metadata)
        {
            var meta = new PackageMeta
            {
                Name = metadata.Id,
                Version = new PackageVersion(metadata.Version.ToString()),
                Title = metadata.Title,
                IconUrl = metadata.IconUrl,
                LicenseUrl = metadata.LicenseUrl,
                ProjectUrl = metadata.ProjectUrl,
                RequireLicenseAcceptance = metadata.RequireLicenseAcceptance,
                Description = metadata.Description,
                Summary = metadata.Summary,
                ReleaseNotes = metadata.ReleaseNotes,
                Language = metadata.Language,
                Tags = metadata.Tags,
                Copyright = metadata.Copyright,
                Listed = metadata.Listed,
                Published = metadata.Published,
                ReportAbuseUrl = metadata.ReportAbuseUrl,
                DownloadCount = metadata.DownloadCount
            };

            meta.Authors.AddRange(metadata.Authors);
            meta.Owners.AddRange(metadata.Owners);

            if (metadata.DependencySetsCount > 1)
            {
                throw new InvalidOperationException("Metadata loaded from nuspec cannot have more than one group of dependency");
            }

            // Load dependencies
            meta.Dependencies.Clear();
            foreach (var dependency in metadata.Dependencies)
            {
                meta.Dependencies.Add(new PackageDependency(dependency.Item1, dependency.Item2));
            }

            return meta;
        }

        public static void ToNugetManifest(PackageMeta meta, ManifestMetadata manifestMeta)
        {
            manifestMeta.Id = meta.Name;
            manifestMeta.Version = meta.Version.ToString();
            manifestMeta.Title = meta.Title.SafeTrim();
            manifestMeta.Authors = string.Join(",", meta.Authors);
            manifestMeta.Owners = string.Join(",", meta.Owners.Count == 0 ? meta.Authors : meta.Owners);
            manifestMeta.Tags = String.IsNullOrEmpty(meta.Tags) ? null : meta.Tags.SafeTrim();
            manifestMeta.LicenseUrl = ConvertUrlToStringSafe(meta.LicenseUrl);
            manifestMeta.ProjectUrl = ConvertUrlToStringSafe(meta.ProjectUrl);
            manifestMeta.IconUrl = ConvertUrlToStringSafe(meta.IconUrl);
            manifestMeta.RequireLicenseAcceptance = meta.RequireLicenseAcceptance;
            manifestMeta.DevelopmentDependency = false;
            manifestMeta.Description = meta.Description.SafeTrim();
            manifestMeta.Copyright = meta.Copyright.SafeTrim();
            manifestMeta.Summary = meta.Summary.SafeTrim();
            manifestMeta.ReleaseNotes = meta.ReleaseNotes.SafeTrim();
            manifestMeta.Language = meta.Language.SafeTrim();

            foreach (var dependency in meta.Dependencies)
            {
                manifestMeta.AddDependency(dependency.Name, dependency.Version);
            }
        }

        private static string ConvertUrlToStringSafe(Uri url)
        {
            if (url != null)
            {
                string originalString = url.OriginalString.SafeTrim();
                if (!string.IsNullOrEmpty(originalString))
                {
                    return originalString;
                }
            }

            return null;
        }
    }
}
