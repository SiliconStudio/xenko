// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Build.Evaluation;

using NuGet;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Internal class to store nuget objects
    /// </summary>
    internal class NugetStore
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

        public void InstallPackage(string packageId, SemanticVersion version)
        {
            using (GetLocalRepositoryLocker())
            {
                Manager.InstallPackage(packageId, version, false, true);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();

                // Install vsix
                ////InstallVsix(GetLatestPackageInstalled(packageId));
            }
        }

        [Obsolete]
        public void UpdatePackage(IPackage package)
        {
            using (GetLocalRepositoryLocker())
            {
                Manager.UpdatePackage(package, true, true);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();

                // Install vsix
                //InstallVsix(GetLatestPackageInstalled(package.Id));
            }
        }

        public void UninstallPackage(IPackage package)
        {
            using (GetLocalRepositoryLocker())
            {
                Manager.UninstallPackage(package);

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

        private List<IPackage> UpdateTargetsInternal()
        {
            var projectCollection = new ProjectCollection();
            var project = new Project(projectCollection);

            var commonPropertyGroup = project.Xml.AddPropertyGroup();
            var target = project.Xml.AddTarget("CheckPackages");
            project.Xml.InitialTargets = "CheckPackages";

            var packages = GetRootPackagesInDependencyOrder();
            foreach (var package in packages)
            {

                if (package.Tags != null && package.Tags.Contains("internal"))
                {
                    continue; // We don't want to polute the Common.targets file with internal packages
                }

                var packageVar = GetPackageVersionVariable(package.Id);
                var packageTarget = String.Format(@"$(MSBuildThisFileDirectory)..\{0}\{1}.{2}\Targets\{1}.targets", RepositoryPath, package.Id, "$(" + packageVar + ")");

                // Add import
                // <Import Project="..\Packages\Xenko$(SiliconStudioPackageXenkoVersion)\Targets\Xenko.targets" Condition="Exists('..\Packages\Xenko.$(SiliconStudioPackageXenkoVersion)\Targets\Xenko.targets')" />
                var importElement = project.Xml.AddImport(packageTarget);
                importElement.Condition = $@"Exists('{packageTarget}')";

                // Add common properties
                var packageVarSaved = packageVar + "Saved";
                var packageVarInvalid = packageVar + "Invalid";
                var packageVarRevision = packageVar + "Revision";
                var packageVarOverride = packageVar + "Override";

                // <SiliconStudioPackageXenkoVersion Condition="'$(SiliconStudioPackageXenkoVersionOverride)' != ''">$(SiliconStudioPackageXenkoVersionOverride)</SiliconStudioPackageXenkoVersion>
                var versionFromOverrideProperty = commonPropertyGroup.AddProperty(packageVar, "$(" + packageVarOverride + ")");
                versionFromOverrideProperty.Condition = "'$(" + packageVarOverride + ")' != ''";

                // <SiliconStudioPackageXenkoVersionSaved>$(SiliconStudioPackageXenkoVersion)</SiliconStudioPackageXenkoVersionSaved>
                commonPropertyGroup.AddProperty(packageVarSaved, "$(" + packageVar + ")");

                // List all the correspondances: Major.minor -> latest installed explicit version

                // Get all the related versions of the same package also installed, and order by Major.Minor
                var allMajorVersions = LocalRepository.FindPackagesById(package.Id).GroupBy(p => p.Version.Version.Major, p => p);
                foreach (var major in allMajorVersions)
                {
                    var majorVersion = major.Key;
                    var minorPkg = major.GroupBy(p => p.Version.Version.Minor, p => p);
                    foreach (var minor in minorPkg)
                    {
                        var latestPackage = minor.First();
                        // <SiliconStudioPackageXenkoVersionRevision Condition="'$(SiliconStudioPackageXenkoVersion)' == '0.5'">0.5.0-alpha09</SiliconStudioPackageXenkoVersionRevision>
                        var revisionVersionProperty = commonPropertyGroup.AddProperty(packageVarRevision, latestPackage.Version.ToString());
                        revisionVersionProperty.Condition = "'$(" + packageVar + ")' == '" + majorVersion + "." + minor.Key + "'";
                    }
                }

                // Replace the version Major.minor by the full revision name
                // <SiliconStudioPackageXenkoVersion>$(SiliconStudioPackageXenkoVersionRevision)</SiliconStudioPackageXenkoVersion>
                commonPropertyGroup.AddProperty(packageVar, "$(" + packageVarRevision + ")");

                // <SiliconStudioPackageXenkoVersionInvalid Condition="'$(SiliconStudioPackageXenkoVersion)' == '' or !Exists('..\Packages\Xenko.$(SiliconStudioPackageXenkoVersion)\Targets\Xenko.targets')">true</SiliconStudioPackageXenkoVersionInvalid>
                commonPropertyGroup.AddProperty(packageVarInvalid, "true").Condition = "'$(" + packageVar + ")' == '' or !" + importElement.Condition;

                // <SiliconStudioPackageXenkoVersion Condition="'$(SiliconStudioPackageXenkoVersionInvalid)' == 'true'">1.0.0-alpha01</SiliconStudioPackageXenkoVersion>
                // Special case: if major version 1.0 still exists, use it as default (new projects should be created with props file)
                var defaultPackageVersion = LocalRepository.FindPackagesById(package.Id).Select(x => x.Version).FirstOrDefault(x => x.Version.Major == 1 && x.Version.Minor == 0) ?? package.Version;
                var invalidProperty = commonPropertyGroup.AddProperty(packageVar, defaultPackageVersion.ToString());
                invalidProperty.Condition = "'$(" + packageVarInvalid + ")' == 'true'";

                // Add in CheckPackages target
                // <Warning Condition="$(SiliconStudioPackageXenkoVersionInvalid) == 'true'"  Text="Package Xenko $(SiliconStudioPackageXenkoVersionSaved) not found. Use version $(SiliconStudioPackageXenkoVersion) instead"/>
                // Disable Warning and use only Message for now
                // TODO: Provide a better diagnostic message (in case the version is really not found or rerouted to a newer version)
                var warningTask = target.AddTask("Message");
                warningTask.Condition = invalidProperty.Condition;
                warningTask.SetParameter("Text", $"Package {package.Id} with version [$({packageVarSaved})] not found. Use version $({packageVar}) instead");
            }

            var targetFile = TargetFile;
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            project.Save(targetFile);

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