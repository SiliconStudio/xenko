// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.PackageManager
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

        private INugetLogger logger;
        private readonly NuGet.PackageManager manager;
        private readonly ISettings settings;
        private readonly IPackagePathResolver pathResolver;

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
            settings = NuGet.Settings.LoadDefaultSettings(rootFileSystem, configFileName, null);

            string installPath = settings.GetRepositoryPath();
            var packagesFileSystem = new PhysicalFileSystem(installPath);
            var packageSourceProvider = new PackageSourceProvider(settings);

            var repositoryFactory = new PackageRepositoryFactory();
            SourceRepository = packageSourceProvider.CreateAggregateRepository(repositoryFactory, true);

            pathResolver = new DefaultPackagePathResolver(packagesFileSystem);

            manager = new NuGet.PackageManager(SourceRepository, pathResolver, packagesFileSystem);
            manager.PackageInstalling += (sender, args) => NugetPackageInstalling?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            manager.PackageInstalled += (sender, args) => NugetPackageInstalled?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            manager.PackageUninstalling += (sender, args) => NugetPackageUninstalling?.Invoke(sender, new NugetPackageOperationEventArgs(args));
            manager.PackageUninstalled += (sender, args) => NugetPackageUninstalled?.Invoke(sender, new NugetPackageOperationEventArgs(args));

            var mainPackageList = settings.GetConfigValue(MainPackagesKey);
            if (string.IsNullOrWhiteSpace(mainPackageList))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{MainPackagesKey}] in config");
            }
            MainPackageIds = mainPackageList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            VSIXPluginId = settings.GetConfigValue(VsixPluginKey);
            if (string.IsNullOrWhiteSpace(VSIXPluginId))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{VsixPluginKey}] in config");
            }

            RepositoryPath = settings.GetConfigValue(RepositoryPathKey);
            if (string.IsNullOrWhiteSpace(RepositoryPath))
            {
                RepositoryPath = DefaultGamePackagesDirectory;
            }

            // Setup NugetCachePath in the cache folder
            Environment.SetEnvironmentVariable("NuGetCachePath", Path.Combine(rootDirectory, "Cache", RepositoryPath));
        }

        public string RootDirectory { get; }

        public IReadOnlyCollection<string> MainPackageIds { get; }

        public string VSIXPluginId { get; }

        public string RepositoryPath { get; }

        public string TargetFile => Path.Combine(RootDirectory, DefaultTargets);

        public INugetLogger Logger
        {
            get
            {
                return logger ?? NugetLogger.NullInstance;
            }

            set
            {
                logger = value;
                manager.Logger = new NugetLogger(logger);
                SourceRepository.Logger = new NugetLogger(logger);
            }
        }

        public AggregateRepository SourceRepository { get; }

        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageInstalled;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageInstalling;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageUninstalled;
        public event EventHandler<NugetPackageOperationEventArgs> NugetPackageUninstalling;

        public string GetInstallPath(NugetPackage package)
        {
            return pathResolver.GetInstallPath(package.IPackage);
        }

        public NugetPackage GetLatestPackageInstalled(IEnumerable<string> packageIds)
        {
            // TODO: we return the first entry, not necessaryly the one that the callers actually want.
            return GetPackagesInstalled(packageIds).FirstOrDefault();
        }

        public IList<NugetPackage> GetPackagesInstalled(IEnumerable<string> packageIds)
        {
            var l = new List<NugetPackage>();
            foreach (var package in manager.LocalRepository.GetPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version))
            {
                l.Add(new NugetPackage(package));
            }
            return l;
        }

        public static string GetPackageVersionVariable(string packageId)
        {
            if (packageId == null) throw new ArgumentNullException(nameof(packageId));
            var newPackageId = packageId.Replace(".", String.Empty);
            return "SiliconStudioPackage" + newPackageId + "Version";
        }

        private IDisposable GetLocalRepositoryLocker()
        {
            return FileLock.Wait("nuget.lock");
        }

        public static bool IsStoreDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            var storeConfig = Path.Combine(directory, DefaultConfig);
            return File.Exists(storeConfig);
        }

        public void UpdateTargets()
        {
            using (GetLocalRepositoryLocker())
            {
                UpdateTargetsInternal();
            }
        }

        private List<IPackage> UpdateTargetsInternal()
        {
            // We don't want to polute the Common.targets file with internal packages
            var packages = GetRootPackagesInDependencyOrder().Where(package => !(package.Tags != null && package.Tags.Contains("internal"))).ToList();

            // Generate target file
            var targetGenerator = new TargetGenerator(this, packages);
            var targetFileContent = targetGenerator.TransformText();

            var targetFile = TargetFile;
            var targetFilePath = Path.GetDirectoryName(targetFile);

            // Make sure directory exists
            if (!Directory.Exists(targetFilePath))
                Directory.CreateDirectory(targetFilePath);

            File.WriteAllText(targetFile, targetFileContent, Encoding.UTF8);

            return packages;
        }

        private List<IPackage> GetRootPackagesInDependencyOrder()
        {
            var packagesInOrder = new List<IPackage>();

            // Get all packages
            var packages = new HashSet<IPackage>();
            foreach (var package in manager.LocalRepository.GetPackages().OrderBy(p => p.Id).ThenByDescending(p => p.Version))
            {
                if (packages.All(p => p.Id != package.Id))
                {
                    packages.Add(package);
                }
            }

            while (packages.Count > 0)
            {
                var nextPackage = packages.FirstOrDefault();
                AddPackageRecursive(packagesInOrder, packages, nextPackage);
            }

            return packagesInOrder;
        }

        private void AddPackageRecursive(List<IPackage> packagesOut, HashSet<IPackage> packages, IPackage packageToTrack)
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

        public string GetPackageDirectory(NugetPackage xenkoPackage)
        {
            return pathResolver.GetPackageDirectory(xenkoPackage.IPackage);
        }

        public string GetPackageDirectory(string packageId, NugetSemanticVersion version)
        {
            return pathResolver.GetPackageDirectory(packageId, version.SemanticVersion);
        }

        public string GetMainExecutables()
        {
            return settings.GetConfigValue(MainExecutablesKey);
        }

        public string GetPrerequisitesInstaller()
        {
            return settings.GetConfigValue(PrerequisitesInstallerKey);
        }

#region Manager
        public void InstallPackage(string packageId, NugetSemanticVersion version)
        {
            using (GetLocalRepositoryLocker())
            {
                manager.InstallPackage(packageId, version.SemanticVersion, false, true);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();

                // Install vsix
                ////InstallVsix(GetLatestPackageInstalled(packageId));
            }
        }

        public void UninstallPackage(NugetPackage package)
        {
            using (GetLocalRepositoryLocker())
            {
                manager.UninstallPackage(package.IPackage);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();
            }
        }

        public IEnumerable<NugetPackage> GetLocalPackages()
        {
            return ToNugetPackages(manager.LocalRepository.GetPackages()).AsQueryable();
        }

        public NugetPackage FindLocalPackage(string packageId, NugetVersionSpec versionSpec, NugetConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            var package = manager.LocalRepository.FindPackage(packageId, versionSpec?.VersionSpec, (IPackageConstraintProvider)constraintProvider?.Provider ?? NullConstraintProvider.Instance, allowPrereleaseVersions, allowUnlisted);
            return package != null ? new NugetPackage(package) : null;
        }

        public IEnumerable<NugetPackage> FindLocalPackages(IReadOnlyCollection<string> packageIds)
        {
            return ToNugetPackages(manager.LocalRepository.FindPackages(packageIds));
        }
        public IEnumerable<NugetPackage> FindLocalPackagesById(string packageId)
        {
            return ToNugetPackages(manager.LocalRepository.FindPackagesById(packageId));
        }

        public IEnumerable<NugetPackage> FindSourcePackages(IReadOnlyCollection<string> packageIds)
        {
            return ToNugetPackages(manager.SourceRepository.FindPackages(packageIds));
        }

        public IEnumerable<NugetPackage> FindSourcePackagesById(string packageId)
        {
            return ToNugetPackages(manager.SourceRepository.FindPackagesById(packageId));
        }
        public IQueryable<NugetPackage> SourceSearch(string searchTerm, bool allowPrereleaseVersions)
        {
            return ToNugetPackages(manager.SourceRepository.Search(searchTerm, allowPrereleaseVersions)).AsQueryable();
        }

        public async Task<IEnumerable<NugetPackage>> GetUpdates(NugetPackageName nugetPackageName, bool includePrerelease, bool includeAllVersions, CancellationToken cancellationToken)
        {
            var list = manager.SourceRepository.GetUpdates(new [] {nugetPackageName.Name}, includePrerelease, includeAllVersions);
            var res = new List<NugetPackage>();
            foreach (var package in list)
            {
                res.Add(new NugetPackage(package));
            }
            return res;
        }

        private IEnumerable<NugetPackage> ToNugetPackages(IEnumerable<IPackage> packages)
        {
            var res = new List<NugetPackage>();
            foreach (var package in packages)
            {
                res.Add(new NugetPackage(package));
            }
            return res;
        }
#endregion
    }
}
