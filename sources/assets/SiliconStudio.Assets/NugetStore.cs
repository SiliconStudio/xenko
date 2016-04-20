// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NuGet;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Internal class to store nuget objects
    /// </summary>
    internal partial class NugetStore
    {
        private const string RepositoryPathKey = "repositorypath";

        private const string MainPackagesKey = "mainPackages";

        private const string VsixPluginKey = "vsixPlugin";

        private const string DefaultTargets = @"Targets\SiliconStudio.Common.targets";

        public const string DefaultGamePackagesDirectory = "GamePackages";

        public const string DefaultConfig = "store.config";

        public const string OverrideConfig = "store.local.config";

        private readonly PhysicalFileSystem rootFileSystem;
        private readonly IFileSystem packagesFileSystem;
        private readonly PackageSourceProvider packageSourceProvider;
        private readonly DefaultPackagePathResolver pathResolver;
        private readonly PackageRepositoryFactory repositoryFactory;
        private ILogger logger;

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

            rootFileSystem = new PhysicalFileSystem(rootDirectory);
            Settings = NuGet.Settings.LoadDefaultSettings(rootFileSystem, configFileName, null);

            string installPath = Settings.GetRepositoryPath();
            packagesFileSystem = new PhysicalFileSystem(installPath);
            packageSourceProvider = new PackageSourceProvider(Settings);

            repositoryFactory = new PackageRepositoryFactory();
            SourceRepository = packageSourceProvider.CreateAggregateRepository(repositoryFactory, true);

            pathResolver = new DefaultPackagePathResolver(packagesFileSystem);

            Manager = new PackageManager(SourceRepository, pathResolver, packagesFileSystem);

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

        public string RootDirectory => rootFileSystem.Root;

        public IReadOnlyCollection<string> MainPackageIds { get; }

        public string VSIXPluginId { get; }

        public string RepositoryPath { get; }

        public string TargetFile => Path.Combine(RootDirectory, DefaultTargets);

        public ILogger Logger
        {
            get
            {
                return logger ?? NullLogger.Instance;
            }

            set
            {
                logger = value;
                Manager.Logger = logger;
                SourceRepository.Logger = logger;
            }
        }

        public ISettings Settings { get; }

        public IPackagePathResolver PathResolver => pathResolver;

        public PackageManager Manager { get; }

        public IPackageRepository LocalRepository => Manager.LocalRepository;

        public AggregateRepository SourceRepository { get; }

        public bool CheckSource()
        {
            return SourceRepository.Repositories.Any(CheckSource);
        }

        public IPackage GetLatestPackageInstalled(string packageId)
        {
            return LocalRepository.GetPackages().Where(p => p.Id == packageId).OrderByDescending(p => p.Version).FirstOrDefault();
        }

        public IList<IPackage> GetPackagesInstalled(string packageId)
        {
            return LocalRepository.GetPackages().Where(p => p.Id == packageId).OrderByDescending(p => p.Version).ToArray();
        }

        public IPackage GetLatestPackageInstalled(IEnumerable<string> packageIds)
        {
            return LocalRepository.GetPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version).FirstOrDefault();
        }

        public IList<IPackage> GetPackagesInstalled(IEnumerable<string> packageIds)
        {
            return LocalRepository.GetPackages().Where(p => packageIds.Any(x => x == p.Id)).OrderByDescending(p => p.Version).ToArray();
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
    }
}