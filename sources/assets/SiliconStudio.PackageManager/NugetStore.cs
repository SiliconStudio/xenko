// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NuGet;
using SiliconStudio.Core.Windows;
using SiliconStudio.PackageManager;

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
            Settings = NuGet.Settings.LoadDefaultSettings(rootFileSystem, configFileName, null);

            string installPath = Settings.GetRepositoryPath();
            var packagesFileSystem = new PhysicalFileSystem(installPath);
            var packageSourceProvider = new PackageSourceProvider(Settings);

            var repositoryFactory = new PackageRepositoryFactory();
            SourceRepository = packageSourceProvider.CreateAggregateRepository(repositoryFactory, true);

            var pathResolver = new DefaultPackagePathResolver(packagesFileSystem);
            PathResolver = pathResolver;

            Manager = new NugetPackageManager(new NuGet.PackageManager(SourceRepository, pathResolver, packagesFileSystem));

            var mainPackageList = Settings.GetConfigValue(MainPackagesKey);
            if (string.IsNullOrWhiteSpace(mainPackageList))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{MainPackagesKey}] in config");
            }
            MainPackageIds = mainPackageList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            VSIXPluginId = Settings.GetConfigValue(VsixPluginKey);
            if (string.IsNullOrWhiteSpace(VSIXPluginId))
            {
                throw new InvalidOperationException($"Invalid configuration. Expecting [{VsixPluginKey}] in config");
            }

            RepositoryPath = Settings.GetConfigValue(RepositoryPathKey);
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
                Manager.Logger = new NugetLogger(logger);
                SourceRepository.Logger = new NugetLogger(logger);
            }
        }

        public ISettings Settings { get; }

        public IPackagePathResolver PathResolver { get; }

        public NugetPackageManager Manager { get; }

        public IPackageRepository LocalRepository => Manager.LocalRepository;

        public AggregateRepository SourceRepository { get; }

        public bool CheckSource()
        {
            return SourceRepository.Repositories.Any(CheckSource);
        }

        public NugetPackage GetLatestPackageInstalled(string packageId)
        {
            return new NugetPackage(LocalRepository.GetPackages().Where(p => p.Id == packageId).OrderByDescending(p => p.Version).FirstOrDefault());
        }

        public string GetInstallPath(NugetPackage package)
        {
            return PathResolver.GetInstallPath(package.IPackage);
        }

        public NugetPackage GetLatestPackageInstalled(IEnumerable<string> packageIds)
        {
            return new NugetPackage(LocalRepository.GetPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version).FirstOrDefault());
        }

        public IList<NugetPackage> GetPackagesInstalled(IEnumerable<string> packageIds)
        {
            var l = new List<NugetPackage>();
            foreach (var package in LocalRepository.GetPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version))
            {
                l.Add(new NugetPackage(package));
            }
            return l;
        }

        public static bool CheckSource(IPackageRepository repository)
        {
            try
            {
                repository.GetPackages().FirstOrDefault();
                return true;
            }
            catch (Exception ex)
            {
                if (!IsSourceUnavailableException(ex))
                {
                    throw;
                }
            }
            return false;
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

        public static bool IsSourceUnavailableException(Exception ex)
        {
            return (((ex is WebException) ||
                (ex.InnerException is WebException) ||
                (ex.InnerException is InvalidOperationException)));
        }

        public static bool IsStoreDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            var storeConfig = Path.Combine(directory, DefaultConfig);
            return File.Exists(storeConfig);
        }

        public void InstallPackage(string packageId, NugetSemanticVersion version)
        {
            using (GetLocalRepositoryLocker())
            {
                Manager.InstallPackage(packageId, version.SemanticVersion, false, true);

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
                Manager.UninstallPackage(package.IPackage);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();
            }
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
            foreach (var package in LocalRepository.GetPackages().OrderBy(p => p.Id).ThenByDescending(p => p.Version))
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
            return PathResolver.GetPackageDirectory(xenkoPackage.IPackage);
        }

        public string GetPackageDirectory(string packageId, NugetSemanticVersion version)
        {
            return PathResolver.GetPackageDirectory(packageId, version.SemanticVersion);
        }

        public string GetMainExecutables()
        {
            return Settings.GetConfigValue(MainExecutablesKey);
        }

        public string GetPrerequisitesInstaller()
        {
            return Settings.GetConfigValue(PrerequisitesInstallerKey);
        }
    }
}