// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Core;
using SiliconStudio.Core.Windows;
// Nuget v2.0 types
using ISettings = NuGet.ISettings;
using Settings = NuGet.Settings;
using PhysicalFileSystem = NuGet.PhysicalFileSystem;
using AggregateRepository = NuGet.AggregateRepository;
using PackageSourceProvider = NuGet.PackageSourceProvider;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Internal class to store nuget objects
    /// </summary>
    public class NugetStore
    {
        private const string RepositoryPathKey = "repositorypath";

        private const string MainPackagesKey = "mainPackages";

        private const string VsixPluginKey = "vsixPlugin";

        private const string DefaultTargets = @"Targets\SiliconStudio.Common.targets";

        public const string DefaultGamePackagesDirectory = "GamePackages";

        public const string DefaultConfig = "store.config";

        public const string OverrideConfig = "store.local.config";

        public const string MainExecutablesKey = "mainExecutables";
        public const string PrerequisitesInstallerKey = "prerequisitesInstaller";

        private IPackagesLogger logger;
        private readonly NuGet.PackageManager manager;
        private readonly ISettings settings;
        private ProgressReport currentProgressReport;

        private static Regex powerShellProgressRegex = new Regex(@".*\[ProgressReport:\s*(\d*)%\].*");

        /// <summary>
        /// Initialize NugetStore using <paramref name="rootDirectory"/> as location of the local copies,
        /// and a configuration file <paramref name="configFile"/> as well as an override configuration
        /// file <paramref name="overrideFile"/> where all settings of <paramref name="overrideFile"/> also
        /// presents in <paramref name="configFile"/> take precedence. 
        /// </summary>
        /// <param name="rootDirectory">The location of the Nuget store.</param>
        /// <param name="configFile">The configuration file name for the Nuget store, or <see cref="DefaultConfig"/> if not specified.</param>
        /// <param name="overrideFile">The override configuration file name for the Nuget store, or <see cref="OverrideConfig"/> if not specified.</param>
        public NugetStore(string rootDirectory, string configFile = DefaultConfig, string overrideFile = OverrideConfig)
        {
            if (rootDirectory == null) throw new ArgumentNullException(nameof(rootDirectory));
            if (configFile == null) throw new ArgumentNullException(nameof(configFile));
            if (overrideFile == null) throw new ArgumentNullException(nameof(overrideFile));

            // First try the override file with custom settings
            var configFileName = overrideFile;
            var configFilePath = Path.Combine(rootDirectory, configFileName);

            if (!File.Exists(configFilePath))
            {
                // Override file does not exist, fallback to default config file
                configFileName = configFile;
                configFilePath = Path.Combine(rootDirectory, configFileName);

                if (!File.Exists(configFilePath))
                {
                    throw new ArgumentException($"Invalid installation. Configuration file [{configFile}] not found", nameof(configFile));
                }
            }

            var rootFileSystem = new PhysicalFileSystem(rootDirectory);
            RootDirectory = rootFileSystem.Root;
            settings = Settings.LoadDefaultSettings(rootFileSystem, configFileName, null);

            InstallPath = settings.GetValue(ConfigurationConstants.Config, RepositoryPathKey, true);
            if (!string.IsNullOrEmpty(InstallPath))
            {
                InstallPath = InstallPath.Replace('/', Path.DirectorySeparatorChar);
            }

            var mainPackageList = settings.GetValue(ConfigurationConstants.Config, MainPackagesKey, false);
            if (string.IsNullOrWhiteSpace(mainPackageList))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{MainPackagesKey}] in config");
            }
            MainPackageIds = mainPackageList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            VsixPluginId = settings.GetValue(ConfigurationConstants.Config, VsixPluginKey, false);
            if (string.IsNullOrWhiteSpace(VsixPluginId))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{VsixPluginKey}] in config");
            }

            RepositoryPath = settings.GetValue(ConfigurationConstants.Config, RepositoryPathKey, false);
            if (string.IsNullOrWhiteSpace(RepositoryPath))
            {
                RepositoryPath = DefaultGamePackagesDirectory;
            }

            // Setup NugetCachePath in the cache folder
            CacheDirectory = Path.Combine(rootDirectory, "Cache");
            Environment.SetEnvironmentVariable("NuGetCachePath", CacheDirectory);

            var packagesFileSystem = new PhysicalFileSystem(InstallPath);
            PathResolver = new PackagePathResolver(packagesFileSystem);

            var packageSourceProvider = new PackageSourceProvider(settings);
            SourceRepository = packageSourceProvider.CreateAggregateRepository(new PackageRepositoryFactory() , true);

            var localRepo = new SharedPackageRepository(PathResolver, packagesFileSystem, rootFileSystem);
            manager = new NuGet.PackageManager(SourceRepository, PathResolver, packagesFileSystem, localRepo);
            manager.PackageInstalling += OnPackageInstalling;
            manager.PackageInstalled += OnPackageInstalled;
            manager.PackageUninstalling += OnPackageUninstalling;
            manager.PackageUninstalled += OnPackageUninstalled;
        }

        /// <summary>
        /// Path under which all packages will be installed or cached.
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// Path under which all packages are downloaded and kept before being installed.
        /// </summary>
        public string CacheDirectory { get; }

        /// <summary>
        /// Path where all packages are installed.
        /// Usually `InstallPath = RootDirectory/RepositoryPath`.
        /// </summary>
        public string InstallPath { get; }

        /// <summary>
        /// Name of folder we use to identify the local repository.
        /// </summary>
        public string RepositoryPath { get; }

        /// <summary>
        /// List of package Ids under which the main package is known. Usually just one entry, but
        /// we could have several in case there is a product name change.
        /// </summary>
        public IReadOnlyCollection<string> MainPackageIds { get; }

        /// <summary>
        /// Package Id of the Visual Studio Integration plugin.
        /// </summary>
        public string VsixPluginId { get; }

        /// <summary>
        /// Path to the Common.targets file. This files list all installed versions
        /// </summary>
        public string TargetFile => Path.Combine(RootDirectory, DefaultTargets);

        /// <summary>
        /// Logger for all operations of the package manager.
        /// </summary>
        public IPackagesLogger Logger
        {
            get
            {
                return logger ?? NullPackagesLogger.Instance;
            }

            set
            {
                logger = value;
                manager.Logger = new NugetLogger(logger);
                SourceRepository.Logger = new NugetLogger(logger);
            }
        }

        /// <summary>
        /// Set of repositories used as source for installing packages.
        /// </summary>
        public AggregateRepository SourceRepository { get; }

        /// <summary>
        /// Helper to locate packages.
        /// </summary>
        private PackagePathResolver PathResolver { get; }

        public event EventHandler<PackageOperationEventArgs> NugetPackageInstalled;
        public event EventHandler<PackageOperationEventArgs> NugetPackageInstalling;
        public event EventHandler<PackageOperationEventArgs> NugetPackageUninstalled;
        public event EventHandler<PackageOperationEventArgs> NugetPackageUninstalling;

        /// <summary>
        /// Installation path of <paramref name="package"/>
        /// </summary>
        /// <param name="package">Package to query.</param>
        /// <returns>The installation path if installed, null otherwise.</returns>
        public string GetInstallPath(NugetPackage package)
        {
            return PathResolver.GetInstallPath(package.IPackage);
        }

        /// <summary>
        /// Name of the directory containing the <paramref name="package"/>.
        /// </summary>
        /// <param name="package">Package to query.</param>
        /// <returns>The name of the package directory.</returns>
        public string GetPackageDirectory(NugetPackage package)
        {
            return PathResolver.GetPackageDirectory(package.IPackage);
        }

        /// <summary>
        /// Get the most recent version associated to <paramref name="packageIds"/>. To make sense
        /// it is assumed that packageIds represent the same package under a different name.
        /// </summary>
        /// <param name="packageIds">List of Ids representing a package name.</param>
        /// <returns>The most recent version of `GetPackagesInstalled (packageIds)`.</returns>
        public NugetPackage GetLatestPackageInstalled(IEnumerable<string> packageIds)
        {
            return GetPackagesInstalled(packageIds).FirstOrDefault();
        }

        /// <summary>
        /// List of all packages represented by <paramref name="packageIds"/>. The list is ordered
        /// from the most recent version to the oldest.
        /// </summary>
        /// <param name="packageIds">List of Ids representing the package names to retrieve.</param>
        /// <returns>The list of packages sorted from the most recent to the oldest.</returns>
        public IList<NugetPackage> GetPackagesInstalled(IEnumerable<string> packageIds)
        {
            return GetLocalPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version).ToList();
        }

        /// <summary>
        /// List of all installed packages.
        /// </summary>
        /// <returns>A list of packages.</returns>
        public IEnumerable<NugetPackage> GetLocalPackages()
        {
            return ToNugetPackages(manager.LocalRepository.GetPackages());
        }

        /// <summary>
        /// Name of variable used to hold the version of <paramref name="packageId"/>.
        /// </summary>
        /// <param name="packageId">The package Id.</param>
        /// <returns>The name of the variable holding the version of <paramref name="packageId"/>.</returns>
        public static string GetPackageVersionVariable(string packageId)
        {
            if (packageId == null) throw new ArgumentNullException(nameof(packageId));
            var newPackageId = packageId.Replace(".", String.Empty);
            return "SiliconStudioPackage" + newPackageId + "Version";
        }

        /// <summary>
        /// Lock to ensure atomicity of updates to the local repository.
        /// </summary>
        /// <returns>A Lock.</returns>
        private IDisposable GetLocalRepositoryLock()
        {
            return FileLock.Wait("nuget.lock");
        }

        /// <summary>
        /// Is <paramref name="directory"/> a location that has a <see cref="DefaultConfig"/> file?
        /// </summary>
        /// <param name="directory">Directory to check.</param>
        /// <returns><c>true</c> if <paramref name="directory"/> has a <see cref="DefaultConfig"/>, <c>false</c> otherwise.</returns>
        public static bool IsStoreDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));

            var storeConfig = Path.Combine(directory, DefaultConfig);
            return File.Exists(storeConfig);
        }

        /// <summary>
        /// Update <see cref="TargetFile"/> content with the list of non-internal packages
        /// that is used to build a solution against a specific revision and handle the case
        /// that a revision does not exist anymore.
        /// This can be safely called from multiple instance as it is protected via a <see cref="FileLock"/>.
        /// </summary>
        public void UpdateTargets()
        {
            using (GetLocalRepositoryLock())
            {
                UpdateTargetsHelper();
            }
        }

        /// <summary>
        /// See <see cref="UpdateTargets"/>. This is the non-concurrent version, always make sure
        /// to hold the lock for the local repository.
        /// </summary>
        private void UpdateTargetsHelper()
        {
            // We don't want to polute the Common.targets file with internal packages
            var packages = GetRootPackagesInDependencyOrder().Where(package => !(package.Tags != null && package.Tags.Contains("internal"))).ToList();

            // Generate target file
            var targetGenerator = new TargetGenerator(this, packages);
            var targetFileContent = targetGenerator.TransformText();

            var targetFile = TargetFile;
            var targetFilePath = Path.GetDirectoryName(targetFile);

            // Make sure directory exists
            if (targetFilePath != null && !Directory.Exists(targetFilePath))
                Directory.CreateDirectory(targetFilePath);

            File.WriteAllText(targetFile, targetFileContent, Encoding.UTF8);
        }

        /// <summary>
        /// Ignoring version numbers, list packages in a pseudo topological order
        /// where a package is listed before its dependencies, unless they have already
        /// been listed.
        /// </summary>
        /// <returns>A list of package in dependency prder.</returns>
        private List<NugetPackage> GetRootPackagesInDependencyOrder()
        {
            var packagesInOrder = new List<NugetPackage>();
            var packages = new HashSet<NugetPackage>();

            // Get all packages and only keep the most recent version for each package Id.
            foreach (var package in GetLocalPackages().OrderBy(p => p.Id).ThenByDescending(p => p.Version))
            {
                if (packages.All(p => p.Id != package.Id))
                {
                    packages.Add(package);
                }
            }

            // For all the found packages, perform a pseudo topological sort starting from
            // the first package we find.
            while (packages.Count > 0)
            {
                var nextPackage = packages.FirstOrDefault();
                AddPackageRecursive(packagesInOrder, packages, nextPackage);
            }

            return packagesInOrder;
        }

        /// <summary>
        /// Add <paramref name="packageToTrack"/> to <paramref name="packagesOut"/> if not already inserted and remove it
        /// from <paramref name="packages"/>. Process is done recursiverly by adding first the dependencies of
        /// <paramref name="packages"/> before the remaining packages in <paramref name="packages"/>.
        /// </summary>
        /// <param name="packagesOut">List of packages processed so far.</param>
        /// <param name="packages">Set of packages remaining to be processed.</param>
        /// <param name="packageToTrack">Current package to check.</param>
        private void AddPackageRecursive(List<NugetPackage> packagesOut, HashSet<NugetPackage> packages, NugetPackage packageToTrack)
        {
            // Go first recursively with all dependencies resolved
            var dependencies = packageToTrack.DependencySets.SelectMany(deps => deps.Dependencies);
            foreach (var dependency in dependencies)
            {
                var nextPackage = packages.FirstOrDefault(p => p.Id == dependency.Id);
                if (nextPackage != null)
                {
                    AddPackageRecursive(packagesOut, packages, nextPackage);
                }
            }

            // This package is now resolved, add it to the ordered list
            packagesOut.Add(packageToTrack);

            // Remove it from the list of packages to process
            packages.Remove(packageToTrack);
        }

        /// <summary>
        /// Name of main executable of current store.
        /// </summary>
        /// <returns>Name of the executable.</returns>
        public string GetMainExecutables()
        {
            return settings.GetValue(ConfigurationConstants.Config, MainExecutablesKey, false);
        }

        /// <summary>
        /// Locate the main executable from a given package installation path. It throws exceptions if not found.
        /// </summary>
        /// <param name="packagePath">The package installation path.</param>
        /// <returns>The main executable.</returns>
        public string LocateMainExecutable(string packagePath)
        {
            var mainExecutableList = GetMainExecutables();
            if (string.IsNullOrWhiteSpace(mainExecutableList))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{NugetStore.MainExecutablesKey}] in config");
            }
            var fullExePath = mainExecutableList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Path.Combine(packagePath, x)).FirstOrDefault(File.Exists);
            if (fullExePath == null)
                throw new InvalidOperationException("Unable to locate the executable for the selected version");

            return fullExePath;
        }

        /// <summary>
        /// Name of prerequisites executable of current store.
        /// </summary>
        /// <returns>Name of the executable.</returns>
        public string GetPrerequisitesInstaller()
        {
            return settings.GetValue(ConfigurationConstants.Config, PrerequisitesInstallerKey, false);
        }

#region Manager
        /// <summary>
        /// Fetch, if not already downloaded, and install the package represented by
        /// (<paramref name="packageId"/>, <paramref name="version"/>).
        /// </summary>
        /// <remarks>It is safe to call it concurrently be cause we operations are done using the FileLock.</remarks>
        /// <param name="packageId">Name of package to install.</param>
        /// <param name="version">Version of package to install.</param>
        /// <param name="progress">Callbacks to report progress of downloads.</param>
        public async Task InstallPackage(string packageId, PackageVersion version, ProgressReport progress)
        {
            using (GetLocalRepositoryLock())
            {
                currentProgressReport = progress;
                try
                {
                    var package = manager.LocalRepository.FindPackage(packageId, version.ToSemanticVersion(), null, allowPrereleaseVersions: true, allowUnlisted: true);
                    if (package == null)
                    {
                        // Let's search in our cache
                        try
                        {
                            package = MachineCache.Default.FindPackage(packageId, version.ToSemanticVersion(), allowPrereleaseVersions: true, allowUnlisted: true);
                        }
                        catch (InvalidDataException)
                        {
                            // Package is somehow corrupted. We ignore this and  will redownload the file.
                        }
                        // It represents the name of the .nupkg in our cache
                        var sourceName = Path.Combine(CacheDirectory, PathResolver.GetPackageFileName(packageId, version.ToSemanticVersion()));
                        if (package == null)
                        {
                            // Always recreate cache in case it was deleted.
                            if (!Directory.Exists(CacheDirectory))
                            {
                                Directory.CreateDirectory(CacheDirectory);
                            }
                            package = manager.SourceRepository.FindPackage(packageId, version.ToSemanticVersion(), NullConstraintProvider.Instance, allowPrereleaseVersions: true, allowUnlisted: true);
                            if (package == null) throw new ApplicationException("Cannot find package");

                            // Package has to be downloaded if it is a DataServicePackage which was not found in our cache.
                            if (package is DataServicePackage)
                            {
                                var downloadPackage = (DataServicePackage) package;
                                var url = downloadPackage.DownloadUrl;
                                var client = new WebClient();
                                var tcs = new TaskCompletionSource<bool>();
                                progress?.UpdateProgress(ProgressAction.Download, 0);
                                client.DownloadProgressChanged += (o, e) => progress?.UpdateProgress(ProgressAction.Download, e.ProgressPercentage);
                                client.DownloadFileCompleted += (o, e) => tcs.SetResult(true);
                                client.DownloadFileAsync(url, sourceName);
                                await tcs.Task;

                                progress?.UpdateProgress(ProgressAction.Download, 100);
                            }
                        }

                        progress?.UpdateProgress(ProgressAction.Install, -1);
                        manager.InstallPackage(package, ignoreDependencies: false, allowPrereleaseVersions: true);

                        OptimizedZipPackage.PurgeCache();
                    }

                    // Every time a new package is installed, we are updating the common targets
                    UpdateTargetsHelper();
                }
                finally
                {
                    currentProgressReport = null;
                }
            }
        }

        /// <summary>
        /// Uninstall <paramref name="package"/>, while still keeping the downloaded file in the cache.
        /// </summary>
        /// <remarks>It is safe to call it concurrently be cause we operations are done using the FileLock.</remarks>
        /// <param name="package">Package to uninstall.</param>
        public void UninstallPackage(NugetPackage package, ProgressReport progress)
        {
            using (GetLocalRepositoryLock())
            {
                currentProgressReport = progress;
                try
                {
                    manager.UninstallPackage(package.IPackage);

                    // Every time a new package is installed, we are updating the common targets
                    UpdateTargetsHelper();
                }
                finally
                {
                    currentProgressReport = null;
                }
            }
        }

        /// <summary>
        /// Find the installed package <paramref name="packageId"/> using the version <paramref name="version"/> if not null, otherwise the <paramref name="constraintProvider"/> if specified.
        /// If no constraints are specified, the first found entry, whatever it means for NuGet, is used.
        /// </summary>
        /// <param name="packageId">Name of the package.</param>
        /// <param name="version">The version.</param>
        /// <param name="constraintProvider">The package constraint provider.</param>
        /// <param name="allowPrereleaseVersions">if set to <c>true</c> [allow prelease version].</param>
        /// <param name="allowUnlisted">if set to <c>true</c> [allow unlisted].</param>
        /// <returns>A Package matching the search criterion or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">packageId</exception>
        /// <returns></returns>
        public NugetPackage FindLocalPackage(string packageId, PackageVersion version = null, ConstraintProvider constraintProvider = null, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
        {
            var package = manager.LocalRepository.FindPackage(packageId, version?.ToSemanticVersion(), constraintProvider?.Provider(), allowPrereleaseVersions, allowUnlisted);
            return package != null ? new NugetPackage(package) : null;
        }

        /// <summary>
        /// Find the installed package <paramref name="packageId"/> using the version <paramref name="versionRange"/> if not null, otherwise the <paramref name="constraintProvider"/> if specified.
        /// If no constraints are specified, the first found entry, whatever it means for NuGet, is used.
        /// </summary>
        /// <param name="packageId">Name of the package.</param>
        /// <param name="versionRange">The version range.</param>
        /// <param name="constraintProvider">The package constraint provider.</param>
        /// <param name="allowPrereleaseVersions">if set to <c>true</c> [allow prelease version].</param>
        /// <param name="allowUnlisted">if set to <c>true</c> [allow unlisted].</param>
        /// <returns>A Package matching the search criterion or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">packageId</exception>
        /// <returns></returns>
        public NugetPackage FindLocalPackage(string packageId, PackageVersionRange versionRange = null, ConstraintProvider constraintProvider = null, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
        {
            var package = manager.LocalRepository.FindPackage(packageId, versionRange?.ToVersionSpec(), constraintProvider?.Provider(), allowPrereleaseVersions, allowUnlisted);
            return package != null ? new NugetPackage(package) : null;
        }

        /// <summary>
        /// Find installed packages with Ids <paramref name="packageIds"/>.
        /// </summary>
        /// <param name="packageIds">List of package Ids we are looking for.</param>
        /// <returns>A list of packages matching <paramref name="packageIds"/> or an empty list if none is found.</returns>
        public IEnumerable<NugetPackage> FindLocalPackages(IReadOnlyCollection<string> packageIds)
        {
            return ToNugetPackages(manager.LocalRepository.FindPackages(packageIds));
        }

        /// <summary>
        /// Find installed packages with Id <paramref name="packageId"/>.
        /// </summary>
        /// <param name="packageId">Id of package we are looking for.</param>
        /// <returns>A list of packages with Id <paramref name="packageId"/> or an empty list if none is found.</returns>
        public IEnumerable<NugetPackage> FindLocalPackagesById(string packageId)
        {
            return ToNugetPackages(manager.LocalRepository.FindPackagesById(packageId));
        }

        /// <summary>
        /// Find available packages from <see cref="SourceRepository"/> with Ids matching <paramref name="packageIds"/>.
        /// </summary>
        /// <param name="packageIds">List of package Ids we are looking for.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of packages matching <paramref name="packageIds"/> or an empty list if none is found.</returns>
        public async Task<IEnumerable<NugetPackage>> FindSourcePackages(IReadOnlyCollection<string> packageIds, CancellationToken cancellationToken)
        {
            return ToNugetPackages(manager.SourceRepository.FindPackages(packageIds));
        }

        /// <summary>
        /// Find available packages from <see cref="SourceRepository"/> with Id matching <paramref name="packageId"/>.
        /// </summary>
        /// <param name="packageId">Id of package we are looking for.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of packages matching <paramref name="packageId"/> or an empty list if none is found.</returns>
         public async Task<IEnumerable<NugetPackage>> FindSourcePackagesById(string packageId, CancellationToken cancellationToken)
        {
            return ToNugetPackages(manager.SourceRepository.FindPackagesById(packageId));
        }

        /// <summary>
        /// Look for available packages from <see cref="SourceRepository"/> with containing <paramref name="searchTerm"/> in either the Id or description of the package.
        /// </summary>
        /// <param name="searchTerm">Term used for search.</param>
        /// <param name="allowPrereleaseVersions">Are we looking in pre-release versions too?</param>
        /// <returns>A list of packages matching <paramref name="searchTerm"/>.</returns>
        public async Task<IQueryable<NugetPackage>> SourceSearch(string searchTerm, bool allowPrereleaseVersions)
        {
            return ToNugetPackages(manager.SourceRepository.Search(searchTerm, allowPrereleaseVersions)).AsQueryable();
        }

        /// <summary>
        /// Returns updates for packages from the repository 
        /// </summary>
        /// <param name="packageName">Package to look for updates</param>
        /// <param name="includePrerelease">Indicates whether to consider prerelease updates.</param>
        /// <param name="includeAllVersions">Indicates whether to include all versions of an update as opposed to only including the latest version.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        public async Task<IEnumerable<NugetPackage>> GetUpdates(PackageName packageName, bool includePrerelease, bool includeAllVersions, CancellationToken cancellationToken)
        {
            var list = manager.SourceRepository.GetUpdates(new [] {new NuGet.PackageName(packageName.Id, new SemanticVersion(packageName.Version.Version, packageName.Version.SpecialVersion))}, includePrerelease, includeAllVersions);
            var res = new List<NugetPackage>();
            foreach (var package in list)
            {
                res.Add(new NugetPackage(package));
            }
            return res;
        }

        /// <summary>
        /// Convenience feature to convert a list fo <see cref="IPackage"/> into a list of <see cref="NugetPackage"/>
        /// </summary>
        /// <param name="packages">List of NuGet packages, possibly null.</param>
        /// <returns>A list possibly empty of <see cref="NugetPackage"/> matching <paramref name="packages"/>.</returns>
        private IEnumerable<NugetPackage> ToNugetPackages(IEnumerable<IPackage> packages)
        {
            var res = new List<NugetPackage>();
            if (packages != null)
            {
                foreach (var package in packages)
                {
                    res.Add(new NugetPackage(package));
                }
            }
            return res;
        }
#endregion


        /// <summary>
        /// Clean all temporary files created thus far during store operations.
        /// </summary>
        public void PurgeCache()
        {
            // Whenever we look at the content of a package or extract it using the default
            // repository, NuGet expands the files in a global temporary folders. It needs to
            // be purged.
            OptimizedZipPackage.PurgeCache();
        }

        private void OnPackageInstalling(object sender, NuGet.PackageOperationEventArgs args)
        {
            NugetPackageInstalling?.Invoke(sender, new PackageOperationEventArgs(args));
        }

        private void OnPackageInstalled(object sender, NuGet.PackageOperationEventArgs args)
        {
            var packageInstallPath = Path.Combine(args.InstallPath, "tools\\packageinstall.exe");
            if (File.Exists(packageInstallPath))
            {
                RunPackageInstall(packageInstallPath, "/install", currentProgressReport);
            }

            NugetPackageInstalled?.Invoke(sender, new PackageOperationEventArgs(args));
        }

        private void OnPackageUninstalling(object sender, NuGet.PackageOperationEventArgs args)
        {
            NugetPackageUninstalling?.Invoke(sender, new PackageOperationEventArgs(args));

            var packageInstallPath = Path.Combine(args.InstallPath, "tools\\packageinstall.exe");
            if (File.Exists(packageInstallPath))
            {
                RunPackageInstall(packageInstallPath, "/uninstall", currentProgressReport);
            }
        }

        private void OnPackageUninstalled(object sender, NuGet.PackageOperationEventArgs args)
        {
            NugetPackageUninstalled?.Invoke(sender, new PackageOperationEventArgs(args));
        }

        private static void RunPackageInstall(string packageInstall, string arguments, ProgressReport progress)
        {
            // Run packageinstall.exe
            using (var process = Process.Start(new ProcessStartInfo(packageInstall, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.GetDirectoryName(packageInstall),
            }))
            {
                if (process == null)
                    throw new InvalidOperationException($"Could not start install package process [{packageInstall}] with options {arguments}");

                var errorOutput = new StringBuilder();

                process.OutputDataReceived += (_, args) =>
                {
                    // Report progress
                    if (progress != null && !string.IsNullOrEmpty(args.Data))
                    {
                        var matches = powerShellProgressRegex.Match(args.Data);
                        int percentageResult;
                        if (matches.Success && int.TryParse(matches.Groups[1].Value, out percentageResult))
                        {
                            progress.UpdateProgress(percentageResult);
                        }
                    }
                };
                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Save errors
                        lock (process)
                        {
                            errorOutput.AppendLine(args.Data);
                        }
                    }
                };

                // Process output and wait for exit
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                // Check exit code
                var exitCode = process.ExitCode;
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Error code {exitCode} while running install package process [{packageInstall}]\n\n" + errorOutput);
                }
            }
        }
    }

    internal class ConfigurationConstants
    {
        /// <summary>
        /// Name of the config section of the NuGet settings file.
        /// </summary>
        internal static readonly string Config = "config";
    }
}
