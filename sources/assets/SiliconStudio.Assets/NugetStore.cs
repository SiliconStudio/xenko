// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using Microsoft.Win32;
using NuGet;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Internal class to store nuget objects
    /// </summary>
    internal class NugetStore
    {
        private const string RepositoryPathKey = "repositorypath";

        private const string MainPackageKey = "mainPackage";

        private const string VsixPluginKey = "vsixPlugin";

        private const string DefaultTargets = @"Targets\SiliconStudio.Common.targets";

        public const string DefaultGamePackagesDirectory = "GamePackages";

        public const string DefaultConfig = "store.config";

        public const string OverrideConfig = "store.local.config";

        private readonly PhysicalFileSystem rootFileSystem;
        private readonly ISettings settings;
        private readonly IFileSystem packagesFileSystem;
        private readonly PackageSourceProvider packageSourceProvider;
        private readonly DefaultPackagePathResolver pathResolver;
        private readonly PackageRepositoryFactory repositoryFactory;
        private readonly AggregateRepository aggregateRepository;
        private readonly PackageManager manager;
        private ILogger logger;

        public NugetStore(string rootDirectory, string configFile = DefaultConfig, string overrideFile = OverrideConfig)
        {
            if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");
            if (configFile == null) throw new ArgumentNullException("configFile");
            if (overrideFile == null) throw new ArgumentNullException("overrideFile");

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
                    throw new ArgumentException(String.Format("Invalid installation. Configuration file [{0}] not found", configFile), "configFile");
                }
            }

            rootFileSystem = new PhysicalFileSystem(rootDirectory);
            settings = NuGet.Settings.LoadDefaultSettings(rootFileSystem, configFileName, null);

            string installPath = settings.GetRepositoryPath();
            packagesFileSystem = new PhysicalFileSystem(installPath);
            packageSourceProvider = new PackageSourceProvider(settings);

            repositoryFactory = new PackageRepositoryFactory();
            aggregateRepository = packageSourceProvider.CreateAggregateRepository(repositoryFactory, true);

            pathResolver = new DefaultPackagePathResolver(packagesFileSystem);

            manager = new PackageManager(aggregateRepository, pathResolver, packagesFileSystem);

            MainPackageId = Settings.GetConfigValue(MainPackageKey);
            if (string.IsNullOrWhiteSpace(MainPackageId))
            {
                throw new InvalidOperationException(string.Format("Invalid configuration. Expecting [{0}] in config", MainPackageKey));
            }

            VSIXPluginId = Settings.GetConfigValue(VsixPluginKey);
            if (string.IsNullOrWhiteSpace(VSIXPluginId))
            {
                throw new InvalidOperationException(string.Format("Invalid configuration. Expecting [{0}] in config", VsixPluginKey));
            }

            RepositoryPath = Settings.GetConfigValue(RepositoryPathKey);
            if (string.IsNullOrWhiteSpace(RepositoryPath))
            {
                RepositoryPath = DefaultGamePackagesDirectory;
            }

            // Setup NugetCachePath in the cache folder
            Environment.SetEnvironmentVariable("NuGetCachePath", Path.Combine(rootDirectory, "Cache", RepositoryPath));
        }

        public string RootDirectory
        {
            get
            {
                return rootFileSystem.Root;
            }
        }

        public string MainPackageId { get; private set; }

        public string VSIXPluginId { get; private set; }

        public string RepositoryPath { get; private set; }

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

        public ISettings Settings
        {
            get
            {
                return settings;
            }
        }

        public IPackagePathResolver PathResolver
        {
            get
            {
                return pathResolver;
            }
        }

        public PackageManager Manager
        {
            get
            {
                return manager;
            }
        }

        public IPackageRepository LocalRepository
        {
            get
            {
                return Manager.LocalRepository;
            }
        }

        public AggregateRepository SourceRepository
        {
            get
            {
                return aggregateRepository;
            }
        }

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

        public void UpdatePackage(IPackage package)
        {
            using (GetLocalRepositoryLocker())
            {
                Manager.UpdatePackage(package, true, true);

                // Every time a new package is installed, we are updating the common targets
                UpdateTargetsInternal();

                // Install vsix
                InstallVsix(GetLatestPackageInstalled(package.Id));
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
            if (packageId == null) throw new ArgumentNullException("packageId");
            var newPackageId = packageId.Replace(".", String.Empty);
            return "SiliconStudioPackage" + newPackageId + "Version";
        }

        private GlobalMutexLocker GetLocalRepositoryLocker()
        {
            return new GlobalMutexLocker("LauncherApp-" + RootDirectory);
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
                // <Import Project="..\Packages\Paradox$(SiliconStudioPackageParadoxVersion)\Targets\Paradox.targets" Condition="Exists('..\Packages\Paradox.$(SiliconStudioPackageParadoxVersion)\Targets\Paradox.targets')" />
                var importElement = project.Xml.AddImport(packageTarget);
                importElement.Condition = String.Format(@"Exists('{0}')", packageTarget);

                // Add common properties
                var packageVarSaved = packageVar + "Saved";
                var packageVarInvalid = packageVar + "Invalid";
                var packageVarRevision = packageVar + "Revision";

                // <SiliconStudioPackageParadoxVersionSaved>$(SiliconStudioPackageParadoxVersion)</SiliconStudioPackageParadoxVersionSaved>
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
                        // <SiliconStudioPackageParadoxVersionRevision Condition="'$(SiliconStudioPackageParadoxVersion)' == '0.5'">0.5.0-alpha09</SiliconStudioPackageParadoxVersionRevision>
                        var revisionVersionProperty = commonPropertyGroup.AddProperty(packageVarRevision, latestPackage.Version.ToString());
                        revisionVersionProperty.Condition = "'$(" + packageVar + ")' == '" + majorVersion + "." + minor.Key + "'";
                    }
                }

                // Replace the version Major.minor by the full revision name
                // <SiliconStudioPackageParadoxVersion>$(SiliconStudioPackageParadoxVersionRevision)</SiliconStudioPackageParadoxVersion>
                commonPropertyGroup.AddProperty(packageVar, "$(" + packageVarRevision + ")");

                // <SiliconStudioPackageParadoxVersionInvalid Condition="'$(SiliconStudioPackageParadoxVersion)' == '' or !Exists('..\Packages\Paradox.$(SiliconStudioPackageParadoxVersion)\Targets\Paradox.targets')">true</SiliconStudioPackageParadoxVersionInvalid>
                commonPropertyGroup.AddProperty(packageVarInvalid, "true").Condition = "'$(" + packageVar + ")' == '' or !" + importElement.Condition;

                // <SiliconStudioPackageParadoxVersion Condition="'$(SiliconStudioPackageParadoxVersionInvalid)' == 'true'">1.0.0-alpha01</SiliconStudioPackageParadoxVersion>
                // Special case: if major version 1.0 still exists, use it as default (new projects should be created with props file)
                var defaultPackageVersion = LocalRepository.FindPackagesById(package.Id).Select(x => x.Version).FirstOrDefault(x => x.Version.Major == 1 && x.Version.Minor == 0) ?? package.Version;
                var invalidProperty = commonPropertyGroup.AddProperty(packageVar, defaultPackageVersion.ToString());
                invalidProperty.Condition = "'$(" + packageVarInvalid + ")' == 'true'";

                // Add in CheckPackages target
                // <Warning Condition="$(SiliconStudioPackageParadoxVersionInvalid) == 'true'"  Text="Package Paradox $(SiliconStudioPackageParadoxVersionSaved) not found. Use version $(SiliconStudioPackageParadoxVersion) instead"/>
                // Disable Warning and use only Message for now
                // TODO: Provide a better diagnostic message (in case the version is really not found or rerouted to a newer version)
                var warningTask = target.AddTask("Message");
                warningTask.Condition = invalidProperty.Condition;
                warningTask.SetParameter("Text", String.Format("Package {0} with version [$({1})] not found. Use version $({2}) instead", package.Id, packageVarSaved, packageVar));
            }

            var targetFile = Path.Combine(RootDirectory, DefaultTargets);
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

        public void InstallVsix(IPackage package)
        {
            if (package == null)
            {
                return;
            }

            var packageDirectory = PathResolver.GetInstallPath(package);
            InstallVsixFromPackageDirectory(packageDirectory);
        }

        internal void InstallVsixFromPackageDirectory(string packageDirectory)
        {
            var vsixInstallerPath = FindLatestVsixInstaller();
            if (vsixInstallerPath == null)
            {
                return;
            }

            var files = Directory.EnumerateFiles(packageDirectory, "*.vsix", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                InstallVsix(vsixInstallerPath, file);
            }
        }

        private void InstallVsix(string vsixInstallerPath, string pathToVsix)
        {
            // Uninstall previous vsix

            var vsixId = GetVsixId(pathToVsix);
            if (vsixId == Guid.Empty)
            {
                throw new InvalidOperationException(string.Format("Invalid VSIX package [{0}]", pathToVsix));
            }

            var vsixName = Path.GetFileNameWithoutExtension(pathToVsix);
            
            // Log just one message when installing the visual studio package
            Logger.Log(MessageLevel.Info, "Installing Visual Studio Package [{0}]", vsixName);

            RunVsixInstaller(vsixInstallerPath, "/q /uninstall:" + vsixId.ToString("D", CultureInfo.InvariantCulture));

            // Install new vsix
            RunVsixInstaller(vsixInstallerPath, "/q \"" + pathToVsix + "\"");
        }

        private static bool RunVsixInstaller(string pathToVsixInstaller, string arguments)
        {
            try
            {
                var process = Process.Start(pathToVsixInstaller, arguments);
                if (process == null)
                {
                    return false;
                }
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private static Guid GetVsixId(string pathToVsix)
        {
            if (pathToVsix == null) throw new ArgumentNullException("pathToVsix");

            var id = Guid.Empty;
            using (var stream = File.OpenRead(pathToVsix))
            {
                var package = System.IO.Packaging.Package.Open(stream);

                var uri = System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri("extension.vsixmanifest", UriKind.Relative));
                var manifest = package.GetPart(uri);

                var doc = XElement.Load(manifest.GetStream());
                var identity = doc.Descendants().FirstOrDefault(element => element.Name.LocalName == "Identity");
                if (identity != null)
                {
                    var idAttribute = identity.Attribute("Id");
                    if (idAttribute != null)
                    {
                        Guid.TryParse(idAttribute.Value, out id);
                    }
                }
            }

            return id;
        }

        private static string FindLatestVsixInstaller()
        {
            var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\");
            if (subKey == null)
            {
                return null;
            }

            var versions = new Dictionary<Version, string>();
            foreach (var subKeyName in subKey.GetSubKeyNames())
            {
                Version version;
                if (Version.TryParse(subKeyName, out version))
                {
                    versions.Add(version, subKeyName);
                }
            }

            foreach (var version in versions.Keys.OrderByDescending(v => v))
            {
                var subKeyName = versions[version];
                
                var vsKey = subKey.OpenSubKey(subKeyName);

                var installDirValue = vsKey.GetValue("InstallDir");
                if (installDirValue != null)
                {
                    var installDir = installDirValue.ToString();
                    var vsixInstallerPath = Path.Combine(installDir, "VSIXInstaller.exe");
                    if (File.Exists(vsixInstallerPath))
                    {
                        return vsixInstallerPath;
                    }
                }
            }

            return null;
        }


        public static bool IsSourceUnavailableException(Exception ex)
        {
            return (((ex is WebException) ||
                (ex.InnerException is WebException) ||
                (ex.InnerException is InvalidOperationException)));
        }

        private class GlobalMutexLocker : IDisposable
        {
            private Mutex mutex;
            private readonly bool owned;

            public GlobalMutexLocker(string name)
            {
                name = name.Replace(":", "_");
                name = name.Replace("/", "_");
                name = name.Replace("\\", "_");
                mutex = new Mutex(true, name, out owned);
                if (!owned)
                {
                    owned = mutex.WaitOne();
                }
            }

            public void Dispose()
            {
                if (owned)
                {
                    mutex.ReleaseMutex();
                }
                mutex = null;
            }
        }

        public static bool IsStoreDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            var storeConfig = Path.Combine(directory, DefaultConfig);
            return File.Exists(storeConfig);
        }
    }
}