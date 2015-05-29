// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Execution;
using SharpYaml;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A session for editing a package.
    /// </summary>
    public sealed class PackageSession : IDisposable, IDirtyable
    {
        private readonly PackageCollection packagesCopy;
        private readonly PackageCollection packages;
        private readonly AssemblyContainer assemblyContainer;
        private readonly object dependenciesLock = new object();
        private Package currentPackage;
        private AssetDependencyManager dependencies;

        public event Action<Asset> AssetDirtyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSession"/> class.
        /// </summary>
        public PackageSession() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageSession"/> class.
        /// </summary>
        public PackageSession(Package package)
        {
            packages = new PackageCollection();
            packagesCopy = new PackageCollection();
            assemblyContainer = new AssemblyContainer();
            packages.CollectionChanged += PackagesCollectionChanged;
            if (package != null)
            {
                Packages.Add(package);
            }            
        }

        /// <inheritdoc/>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <value>The packages.</value>
        public PackageCollection Packages
        {
            get
            {
                return packages;
            }
        }

        /// <summary>
        /// Gets the user packages (excluding system packages).
        /// </summary>
        /// <value>The user packages.</value>
        public IEnumerable<Package> LocalPackages
        {
            get
            {
                return packages.Where(package => !package.IsSystem);
            }
        }

        /// <summary>
        /// Gets or sets the solution path (sln) in case the session was loaded from a solution.
        /// </summary>
        /// <value>The solution path.</value>
        public UFile SolutionPath { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (dependencies != null)
            {
                dependencies.Dispose();
            }

            foreach (var assembly in assemblyContainer.LoadedAssemblies)
            {
                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Unregister(assembly.Value);

                // Unload assembly
                assemblyContainer.UnloadAssembly(assembly.Value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has dependency manager.
        /// </summary>
        /// <value><c>true</c> if this instance has dependency manager; otherwise, <c>false</c>.</value>
        public bool HasDependencyManager
        {
            get
            {
                lock (dependenciesLock)
                {
                    return dependencies != null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected current package.
        /// </summary>
        /// <value>The selected current package.</value>
        /// <exception cref="System.InvalidOperationException">Expecting a package that is already registered in this session</exception>
        public Package CurrentPackage
        {
            get
            {
                return currentPackage;
            }
            set
            {
                if (value != null)
                {
                    if (!Packages.Contains(value))
                    {
                        throw new InvalidOperationException("Expecting a package that is already registered in this session");
                    }
                }
                currentPackage = value;
            }
        }

        /// <summary>
        /// Gets the packages referenced by the current package.
        /// </summary>
        /// <returns>IEnumerable&lt;Package&gt;.</returns>
        public IEnumerable<Package> GetPackagesFromCurrent()
        {
            if (CurrentPackage == null)
            {
                yield break;
            }

            yield return CurrentPackage;

            foreach (var storeDep in CurrentPackage.Meta.Dependencies)
            {
                var package = Packages.Find(storeDep);
                // In case the package is not found (when working with session not fully loaded/resolved with all deps)
                if (package != null)
                {
                    yield return package;
                }
            }

            foreach (var localDep in CurrentPackage.LocalDependencies)
            {
                var package = Packages.Find(localDep.Id);
                // In case the package is not found (when working with session not fully loaded/resolved with all deps)
                if (package != null)
                {
                    yield return package;
                }
            }
        }

        /// <summary>
        /// Gets the dependency manager.
        /// </summary>
        /// <value>AssetDependencyManager.</value>
        public AssetDependencyManager DependencyManager
        {
            get
            {
                lock (dependenciesLock)
                {
                    return dependencies ?? (dependencies = new AssetDependencyManager(this));
                }
            }
        }

        /// <summary>
        /// Adds an existing package to the current session.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="loadParameters">The load parameters.</param>
        /// <exception cref="System.ArgumentNullException">packagePath</exception>
        /// <exception cref="System.IO.FileNotFoundException">Unable to find package</exception>
        public LoggerResult AddExistingPackage(UFile packagePath, PackageLoadParameters loadParameters = null)
        {
            var loggerResult = new LoggerResult();
            AddExistingPackage(packagePath, loggerResult, loadParameters);
            return loggerResult;
        }

        /// <summary>
        /// Adds an existing package to the current session.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="logger">The session result.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        /// <exception cref="System.ArgumentNullException">packagePath</exception>
        /// <exception cref="System.ArgumentException">Invalid relative path. Expecting an absolute package path;packagePath</exception>
        /// <exception cref="System.IO.FileNotFoundException">Unable to find package</exception>
        public Package AddExistingPackage(UFile packagePath, ILogger logger, PackageLoadParameters loadParametersArg = null)
        {
            if (packagePath == null) throw new ArgumentNullException("packagePath");
            if (logger == null) throw new ArgumentNullException("logger");
            if (!packagePath.IsAbsolute) throw new ArgumentException("Invalid relative path. Expecting an absolute package path", "packagePath");
            if (!File.Exists(packagePath)) throw new FileNotFoundException("Unable to find package", packagePath);

            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            Package package;
            try
            {
                // Enable reference analysis caching during loading
                AssetReferenceAnalysis.EnableCaching = true;

                var packagesLoaded = new PackageCollection();

                package = PreLoadPackage(this, logger, packagePath, false, packagesLoaded, loadParameters);

                // Run analysis after
                foreach (var packageToAdd in packagesLoaded)
                {
                    var analysis = new PackageAnalysis(packageToAdd, GetPackageAnalysisParametersForLoad());
                    analysis.Run(logger);
                }
            }
            finally
            {
                // Disable reference analysis caching after loading
                AssetReferenceAnalysis.EnableCaching = false;
            }
            return package;
        }

        /// <summary>
        /// Loads a package from specified file path.
        /// </summary>
        /// <param name="filePath">The file path to a package file.</param>
        /// <param name="sessionResult">The session result.</param>
        /// <param name="loadParameters">The load parameters.</param>
        /// <returns>A package.</returns>
        /// <exception cref="System.ArgumentNullException">filePath</exception>
        /// <exception cref="System.ArgumentException">File [{0}] must exist.ToFormat(filePath);filePath</exception>
        public static void Load(string filePath, PackageSessionResult sessionResult, PackageLoadParameters loadParameters = null)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (sessionResult == null) throw new ArgumentNullException("sessionResult");

            // Make sure with have valid parameters
            loadParameters = loadParameters ?? PackageLoadParameters.Default();

            // Make sure to use a full path.
            filePath = FileUtility.GetAbsolutePath(filePath);

            if (!File.Exists(filePath)) throw new ArgumentException("File [{0}] must exist".ToFormat(filePath), "filePath");

            try
            {
                // Enable reference analysis caching during loading
                AssetReferenceAnalysis.EnableCaching = true;

                using (var profile = Profiler.Begin(PackageSessionProfilingKeys.Loading))
                {
                    sessionResult.Clear();
                    sessionResult.Progress("Loading..", 0, 1);

                    var session = new PackageSession();

                    var packagePaths = new List<string>();

                    // If we have a solution, load all packages
                    if (PackageSessionHelper.IsSolutionFile(filePath))
                    {
                        PackageSessionHelper.LoadSolution(session, filePath, packagePaths, sessionResult);
                    }
                    else if (PackageSessionHelper.IsPackageFile(filePath))
                    {
                        packagePaths.Add(filePath);
                    }
                    else
                    {
                        sessionResult.Error("Unsupported file extension (only .sln or {0} are supported)", Package.PackageFileExtension);
                        return;
                    }

                    var cancelToken = loadParameters.CancelToken;

                    // Load all packages
                    var packagesLoaded = new PackageCollection();
                    foreach (var packageFilePath in packagePaths)
                    {
                        PreLoadPackage(session, sessionResult, packageFilePath, false, packagesLoaded, loadParameters);

                        // Output the session only if there is no cancellation
                        if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    // Load all missing references/dependencies
                    session.LoadMissingReferences(sessionResult, loadParameters);

                    // Fix relative references
                    var analysis = new PackageSessionAnalysis(session, GetPackageAnalysisParametersForLoad());
                    var analysisResults = analysis.Run();
                    analysisResults.CopyTo(sessionResult);

                    // Run custom package session analysis
                    foreach (var type in AssetRegistry.GetPackageSessionAnalysisTypes())
                    {
                        var pkgAnalysis = (PackageSessionAnalysisBase)Activator.CreateInstance(type);
                        pkgAnalysis.Session = session;
                        var results = pkgAnalysis.Run();
                        results.CopyTo(sessionResult);
                    }

                    // Output the session only if there is no cancellation
                    if (!cancelToken.HasValue || !cancelToken.Value.IsCancellationRequested)
                    {
                        sessionResult.Session = session;

                        // Defer the initialization of the dependency manager
                        //session.DependencyManager.InitializeDeferred();
                    }

                    // The session is not dirty when loading it
                    session.IsDirty = false;
                }
            }
            finally
            {
                // Disable reference analysis caching after loading
                AssetReferenceAnalysis.EnableCaching = false;
            }
        }

        /// <summary>
        /// Loads a package from specified file path.
        /// </summary>
        /// <param name="filePath">The file path to a package file.</param>
        /// <param name="loadParameters">The load parameters.</param>
        /// <returns>A package.</returns>
        /// <exception cref="System.ArgumentNullException">filePath</exception>
        public static PackageSessionResult Load(string filePath, PackageLoadParameters loadParameters = null)
        {
            var result = new PackageSessionResult();
            Load(filePath, result, loadParameters);
            return result;
        }

        /// <summary>
        /// Loads the missing references
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        public void LoadMissingReferences(ILogger log, PackageLoadParameters loadParametersArg = null)
        {
            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            var cancelToken = loadParameters.CancelToken;

            var packagesLoaded = new PackageCollection();

            // Make a copy of Packages as it can be modified by PreLoadPackageDependencies
            var previousPackages = Packages.ToList();
            foreach (var package in previousPackages)
            {
                // Output the session only if there is no cancellation
                if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                {
                    return;
                }

                PreLoadPackageDependencies(this, log, package, packagesLoaded, loadParameters);
            }
        }

        /// <summary>
        /// Saves all packages and assets.
        /// </summary>
        /// <returns>Result of saving.</returns>
        public LoggerResult Save()
        {
            var log = new LoggerResult();

            bool packagesSaved = false;

            //var clock = Stopwatch.StartNew();
            using (var profile = Profiler.Begin(PackageSessionProfilingKeys.Saving))
            {
                try
                {
                    // Grab all previous assets
                    var previousAssets = new Dictionary<Guid, AssetItem>();
                    foreach (var assetItem in packagesCopy.SelectMany(package => package.Assets))
                    {
                        previousAssets[assetItem.Id] = assetItem;
                    }

                    // Grab all new assets
                    var newAssets = new Dictionary<Guid, AssetItem>();
                    foreach (var assetItem in LocalPackages.SelectMany(package => package.Assets))
                    {
                        newAssets[assetItem.Id] = assetItem;
                    }

                    // Compute all assets that were removed
                    var assetsOrPackagesToRemove = new Dictionary<UFile, object>();
                    foreach (var assetIt in previousAssets)
                    {
                        var asset = assetIt.Value;

                        AssetItem newAsset;
                        if (!newAssets.TryGetValue(assetIt.Key, out newAsset) || newAsset.Location != asset.Location)
                        {
                            assetsOrPackagesToRemove[asset.FullPath] = asset;
                        }
                    }

                    // Compute packages that have been renamed
                    // TODO: Disable for now, as not sure if we want to delete a previous package
                    //foreach (var package in packagesCopy)
                    //{
                    //    var newPackage = packages.Find(package.Id);
                    //    if (newPackage != null && package.PackagePath != null && newPackage.PackagePath != package.PackagePath)
                    //    {
                    //        assetsOrPackagesToRemove[package.PackagePath] = package;
                    //    }
                    //}

                    // If package are not modified, return immediately
                    if (!CheckModifiedPackages() && assetsOrPackagesToRemove.Count == 0)
                    {
                        return log;
                    }

                    // Suspend tracking when saving as we don't want to receive
                    // all notification events
                    if (dependencies != null)
                    {
                        dependencies.BeginSavingSession();
                    }

                    // Return immediately if there is any error
                    if (log.HasErrors)
                    {
                        return log;
                    }

                    // Delete previous files
                    foreach (var fileIt in assetsOrPackagesToRemove)
                    {
                        var assetPath = fileIt.Key;
                        var assetItemOrPackage = fileIt.Value;

                        if (File.Exists(assetPath))
                        {
                            try
                            {
                                File.Delete(assetPath);
                            }
                            catch (Exception ex)
                            {
                                var assetItem = assetItemOrPackage as AssetItem;
                                if (assetItem != null)
                                {
                                    log.Error(assetItem.Package, assetItem.ToReference(), AssetMessageCode.AssetCannotDelete, ex, assetPath);
                                }
                                else
                                {
                                    var package = assetItemOrPackage as Package;
                                    if (package != null)
                                    {
                                        log.Error(package, null, AssetMessageCode.AssetCannotDelete, ex, assetPath);
                                    }
                                }
                            }
                        }
                    }

                    // Save all dirty assets
                    packagesCopy.Clear();
                    foreach (var package in LocalPackages)
                    {
                        // Save the package to disk and all its assets
                        package.Save(log);

                        // Clone the package (but not all assets inside, just the structure)
                        var packageClone = package.Clone(false);
                        packagesCopy.Add(packageClone);
                    }

                    packagesSaved = true;
                }
                finally
                {
                    if (dependencies != null)
                    {
                        dependencies.EndSavingSession();
                    }

                    // Once all packages and assets have been saved, we can save the solution (as we need to have fullpath to
                    // be setup for the packages)
                    if (packagesSaved)
                    {
                        PackageSessionHelper.SaveSolution(this, log);
                    }
                }

                //System.Diagnostics.Trace.WriteLine("Elapsed saved: " + clock.ElapsedMilliseconds);
                IsDirty = false;

                return log;
            }
        }

        private bool CheckModifiedPackages()
        {
            if (IsDirty)
            {
                return true;
            }

            foreach (var package in LocalPackages)
            {
                if (package.IsDirty || package.Assets.IsDirty)
                {
                    return true;
                }
                if (package.Assets.Any(item => item.IsDirty))
                {
                    return true;
                }
            }
            return false;
        }

        private void PackagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterPackage((Package)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnRegisterPackage((Package)e.OldItems[0]);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    packagesCopy.Clear();

                    foreach (var oldPackage in e.OldItems.OfType<Package>())
                    {
                        UnRegisterPackage(oldPackage);
                    }

                    foreach (var packageToCopy in packages)
                    {
                        RegisterPackage(packageToCopy);
                    }
                    break;
            }
        }

        private void RegisterPackage(Package package)
        {
            package.Session = this;
            if (package.IsSystem)
                return;
            package.AssetDirtyChanged += OnAssetDirtyChanged;

            // If the package doesn't have any temporary assets, we can freeze it
            if (package.TemporaryAssets.Count == 0)
            {
                FreezePackage(package);
            }

            IsDirty = true;
        }

        /// <summary>
        /// Freeze a package once it is loaded with all its assets
        /// </summary>
        /// <param name="package"></param>
        private void FreezePackage(Package package)
        {
            if (package.IsSystem)
                return;

            packagesCopy.Add(package.Clone(false));
        }

        private void UnRegisterPackage(Package package)
        {
            package.Session = null;
            if (package.IsSystem)
                return;
            package.AssetDirtyChanged -= OnAssetDirtyChanged;

            packagesCopy.RemoveById(package.Id);

            IsDirty = true;
        }

        private void OnAssetDirtyChanged(Asset asset)
        {
            Action<Asset> handler = AssetDirtyChanged;
            if (handler != null) handler(asset);
        }

        private static Package PreLoadPackage(PackageSession session, ILogger log, string filePath, bool isSystemPackage, PackageCollection loadedPackages, PackageLoadParameters loadParameters)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (log == null) throw new ArgumentNullException("log");
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (loadedPackages == null) throw new ArgumentNullException("loadedPackages");
            if (loadParameters == null) throw new ArgumentNullException("loadParameters");

            try
            {
                var packageId = Package.GetPackageIdFromFile(filePath);

                // Check that the package was not already loaded, otherwise return the same instance
                if (session.Packages.ContainsById(packageId))
                {
                    return session.Packages.Find(packageId);
                }

                // Package is already loaded, use the instance 
                if (loadedPackages.ContainsById(packageId))
                {
                    return loadedPackages.Find(packageId);
                }

                // Load the package without loading assets
                var newLoadParameters = loadParameters.Clone();
                newLoadParameters.AssemblyContainer = session.assemblyContainer;

                // Load the package
                var package = Package.Load(log, filePath, newLoadParameters);
                package.IsSystem = isSystemPackage;

                // Add the package has loaded before loading dependencies
                loadedPackages.Add(package);

                // Load package dependencies
                PreLoadPackageDependencies(session, log, package, loadedPackages, loadParameters);

                // Add the package to the session but don't freeze it yet
                session.Packages.Add(package);

                // Validate assets from package
                package.ValidateAssets(loadParameters.GenerateNewAssetIds);

                // Freeze the package after loading the assets
                session.FreezePackage(package);

                return package;
            }
            catch (Exception ex)
            {
                log.Error("Error while pre-loading package [{0}]", ex, filePath);
            }

            return null;
        }

        private static void PreLoadPackageDependencies(PackageSession session, ILogger log, Package package, PackageCollection loadedPackages, PackageLoadParameters loadParameters)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (log == null) throw new ArgumentNullException("log");
            if (package == null) throw new ArgumentNullException("package");
            if (loadParameters == null) throw new ArgumentNullException("loadParameters");

            // 1. Load store package
            foreach (var packageDependency in package.Meta.Dependencies)
            {
                var loadedPackage = session.Packages.Find(packageDependency);
                if (loadedPackage != null)
                {
                    continue;
                }

                var file = PackageStore.Instance.GetPackageFileName(packageDependency.Name, packageDependency.Version);

                if (file == null)
                {
                    // TODO: We need to support automatic download of packages. This is not supported yet when only Paradox
                    // package is supposed to be installed, but It will be required for full store
                    log.Error("Unable to find package {0} not installed", packageDependency);
                    continue;
                }

                // Recursive load of the system package
                PreLoadPackage(session, log, file, true, loadedPackages, loadParameters);
            }

            // 2. Load local packages
            foreach (var packageReference in package.LocalDependencies)
            {
                // Check that the package was not already loaded, otherwise return the same instance
                if (session.Packages.ContainsById(packageReference.Id))
                {
                    continue;
                }

                // Expand the string of the location
                var newLocation = (UFile)AssetRegistry.ExpandString(session, packageReference.Location);

                var subPackageFilePath = package.RootDirectory != null ? UPath.Combine(package.RootDirectory, newLocation) : newLocation;

                // Recursive load
                PreLoadPackage(session, log, subPackageFilePath.FullPath, false, loadedPackages, loadParameters);
            }
        }

        private static PackageAnalysisParameters GetPackageAnalysisParametersForLoad()
        {
            return new PackageAnalysisParameters()
            {
                IsPackageCheckDependencies = true,
                IsProcessingAssetReferences = true,
                IsLoggingAssetNotFoundAsError = true,
            };
        }

    }
}