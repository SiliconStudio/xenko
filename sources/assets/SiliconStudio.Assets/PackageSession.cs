// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NuGet;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Core.Reflection;
using ILogger = SiliconStudio.Core.Diagnostics.ILogger;
using Microsoft.Build.Evaluation;

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A session for editing a package.
    /// </summary>
    public sealed class PackageSession : IDisposable
    {
        private readonly DefaultConstraintProvider constraintProvider = new DefaultConstraintProvider();
        private readonly PackageCollection packagesCopy;
        private readonly object dependenciesLock = new object();
        private Package currentPackage;
        private AssetDependencyManager dependencies;

        public event DirtyFlagChangedDelegate<Asset> AssetDirtyChanged;

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
            constraintProvider.AddConstraint(PackageStore.Instance.DefaultPackageName, new VersionSpec(PackageStore.Instance.DefaultPackageVersion.ToSemanticVersion()));

            Packages = new PackageCollection();
            packagesCopy = new PackageCollection();
            AssemblyContainer = new AssemblyContainer();
            Packages.CollectionChanged += PackagesCollectionChanged;
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
        public PackageCollection Packages { get; }

        /// <summary>
        /// Gets the user packages (excluding system packages).
        /// </summary>
        /// <value>The user packages.</value>
        public IEnumerable<Package> LocalPackages => Packages.Where(package => !package.IsSystem);

        /// <summary>
        /// Gets or sets the solution path (sln) in case the session was loaded from a solution.
        /// </summary>
        /// <value>The solution path.</value>
        public UFile SolutionPath { get; set; }

        public AssemblyContainer AssemblyContainer { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            dependencies?.Dispose();

            var loadedAssemblies = Packages.SelectMany(x => x.LoadedAssemblies).ToList();
            for (int index = loadedAssemblies.Count - 1; index >= 0; index--)
            {
                var loadedAssembly = loadedAssemblies[index];
                if (loadedAssembly == null)
                    continue;

                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Unregister(loadedAssembly.Assembly);

                // Unload binary serialization
                DataSerializerFactory.UnregisterSerializationAssembly(loadedAssembly.Assembly);

                // Unload assembly
                AssemblyContainer.UnloadAssembly(loadedAssembly.Assembly);
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
            if (packagePath == null) throw new ArgumentNullException(nameof(packagePath));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (!packagePath.IsAbsolute) throw new ArgumentException(@"Invalid relative path. Expecting an absolute package path", nameof(packagePath));
            if (!File.Exists(packagePath)) throw new FileNotFoundException("Unable to find package", packagePath);

            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            Package package;
            try
            {
                // Enable reference analysis caching during loading
                AssetReferenceAnalysis.EnableCaching = true;

                var packagesLoaded = new PackageCollection();

                package = PreLoadPackage(this, logger, packagePath, false, packagesLoaded, loadParameters);

                // Load all missing references/dependencies
                LoadMissingReferences(logger, loadParameters);

                // Load assets
                TryLoadAssets(this, logger, package, loadParameters);

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
        /// Adds an existing package to the current session and runs the package analysis before adding it.
        /// </summary>
        /// <param name="package">The package to add</param>
        /// <param name="logger">The logger</param>
        public void AddExistingPackage(Package package, ILogger logger)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (Packages.Contains(package))
            {
                return;
            }

            // Preset the session on the package to allow the session to look for existing asset
            Packages.Add(package);

            // Run analysis after
            var analysis = new PackageAnalysis(package, GetPackageAnalysisParametersForLoad());
            analysis.Run(logger);

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
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (sessionResult == null) throw new ArgumentNullException(nameof(sessionResult));

            // Make sure with have valid parameters
            loadParameters = loadParameters ?? PackageLoadParameters.Default();

            // Make sure to use a full path.
            filePath = FileUtility.GetAbsolutePath(filePath);

            if (!File.Exists(filePath)) throw new ArgumentException($"File [{filePath}] must exist", nameof(filePath));

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

                    // Setup the current package when loading it
                    if (packagePaths.Count == 1)
                    {
                        var currentPackagePath = new UFile(packagePaths[0]);
                        foreach (var package in packagesLoaded)
                        {
                            if (package.FullPath == currentPackagePath)
                            {
                                session.CurrentPackage = package;
                                break;
                            }
                        }
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
        /// Make sure packages have their dependencies and assets loaded.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="loadParameters">The load parameters.</param>
        public void LoadMissingReferences(ILogger log, PackageLoadParameters loadParameters = null)
        {
            LoadMissingDependencies(log, loadParameters);
            LoadMissingAssets(log, loadParameters);
        }

        /// <summary>
        /// Make sure packages have their dependencies loaded.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        public void LoadMissingDependencies(ILogger log, PackageLoadParameters loadParametersArg = null)
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
        /// Make sure packages have their assets loaded.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="loadParametersArg">The load parameters argument.</param>
        public void LoadMissingAssets(ILogger log, PackageLoadParameters loadParametersArg = null)
        {
            var loadParameters = loadParametersArg ?? PackageLoadParameters.Default();

            var cancelToken = loadParameters.CancelToken;

            // Make a copy of Packages as it can be modified by PreLoadPackageDependencies
            var previousPackages = Packages.ToList();
            foreach (var package in previousPackages)
            {
                // Output the session only if there is no cancellation
                if (cancelToken.HasValue && cancelToken.Value.IsCancellationRequested)
                {
                    return;
                }

                TryLoadAssets(this, log, package, loadParameters);
            }
        }

        /// <summary>
        /// Saves all packages and assets.
        /// </summary>
        /// <returns>Result of saving.</returns>
        public LoggerResult Save()
        {
            var log = new LoggerResult();
            Save(log);
            return log;
        }

        /// <summary>
        /// Saves all packages and assets.
        /// </summary>
        /// <param name="log">The <see cref="LoggerResult"/> in which to report result.</param>
        /// <param name="saveParameters">The parameters for the save operation.</param>
        public void Save(LoggerResult log, PackageSaveParameters saveParameters = null)
        {
            bool packagesSaved = false;

            //var clock = Stopwatch.StartNew();
            using (var profile = Profiler.Begin(PackageSessionProfilingKeys.Saving))
            {
                try
                {
                    saveParameters = saveParameters ?? PackageSaveParameters.Default();
                    var assetsOrPackagesToRemove = BuildAssetsOrPackagesToRemove();

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
                        return;
                    }

                    // Suspend tracking when saving as we don't want to receive
                    // all notification events
                    dependencies?.SourceTracker.BeginSavingSession();

                    // Return immediately if there is any error
                    if (log.HasErrors)
                        return;
       
                    //batch projects
                    var vsProjs = new Dictionary<string, Project>();

                    // Delete previous files
                    foreach (var fileIt in assetsOrPackagesToRemove)
                    {
                        var assetPath = fileIt.Key;
                        var assetItemOrPackage = fileIt.Value;

                        var assetItem = assetItemOrPackage as AssetItem;

                        if (File.Exists(assetPath))
                        {
                            try
                            {
                                //If we are within a csproj we need to remove the file from there as well
                                if (assetItem?.SourceProject != null)
                                {
                                    var projectAsset = assetItem.Asset as ProjectSourceCodeAsset;
                                    if (projectAsset != null)
                                    {
                                        Project project;
                                        if (!vsProjs.TryGetValue(assetItem.SourceProject, out project))
                                        {
                                            project = VSProjectHelper.LoadProject(assetItem.SourceProject);
                                            vsProjs.Add(assetItem.SourceProject, project);
                                        }
                                        var include = (new UFile(projectAsset.ProjectInclude)).ToWindowsPath();
                                        var item = project.Items.FirstOrDefault(x => (x.ItemType == "Compile" || x.ItemType == "None") && x.EvaluatedInclude == include);
                                        if (item != null)
                                        {
                                            project.RemoveItem(item);
                                        }
                                    }
                                    //delete any generated file as well
                                    var generatorAsset = assetItem.Asset as ProjectCodeGeneratorAsset;
                                    if (generatorAsset?.GeneratedAbsolutePath != null)
                                    {
                                        File.Delete((new UFile(generatorAsset.GeneratedAbsolutePath)).ToWindowsPath());

                                        //and remove from project as well
                                        Project project;
                                        if (!vsProjs.TryGetValue(assetItem.SourceProject, out project))
                                        {
                                            project = VSProjectHelper.LoadProject(assetItem.SourceProject);
                                            vsProjs.Add(assetItem.SourceProject, project);
                                        }
                                        var include = new UFile(new UFile(projectAsset.ProjectInclude).GetFullPathWithoutExtension() + ".cs").ToWindowsPath();
                                        var item = project.Items.FirstOrDefault(x => (x.ItemType == "Compile" || x.ItemType == "None") && x.EvaluatedInclude == include);
                                        if (item != null)
                                        {
                                            project.RemoveItem(item);
                                        }
                                    }
                                }

                                File.Delete(assetPath);
                            }
                            catch (Exception ex)
                            {
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

                    foreach (var project in vsProjs.Values)
                    {
                        project.Save();
                        project.ProjectCollection.UnloadAllProjects();
                        project.ProjectCollection.Dispose();
                    }

                    // Save all dirty assets
                    packagesCopy.Clear();
                    foreach (var package in LocalPackages)
                    {
                        // Save the package to disk and all its assets
                        package.Save(log);

                        // Clone the package (but not all assets inside, just the structure)
                        var packageClone = package.Clone();
                        packagesCopy.Add(packageClone);
                    }

                    packagesSaved = true;
                }
                finally
                {
                    dependencies?.SourceTracker.EndSavingSession();

                    // Once all packages and assets have been saved, we can save the solution (as we need to have fullpath to
                    // be setup for the packages)
                    if (packagesSaved)
                    {
                        PackageSessionHelper.SaveSolution(this, log);
                    }
                }

                //System.Diagnostics.Trace.WriteLine("Elapsed saved: " + clock.ElapsedMilliseconds);
                IsDirty = false;
            }
        }

        private Dictionary<UFile, object> BuildAssetsOrPackagesToRemove()
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
            return assetsOrPackagesToRemove;
        }

        /// <summary>
        /// Loads the assembly references that were not loaded before.
        /// </summary>
        /// <param name="log">The log.</param>
        public void UpdateAssemblyReferences(LoggerResult log)
        {
            foreach (var package in LocalPackages)
            {
                package.UpdateAssemblyReferences(log);
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

                    foreach (var packageToCopy in Packages)
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

            // Freeze only when assets are loaded
            if (package.State < PackageState.AssetsReady)
                return;

            packagesCopy.Add(package.Clone());
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

        private void OnAssetDirtyChanged(Asset asset, bool oldValue, bool newValue)
        {
            AssetDirtyChanged?.Invoke(asset, oldValue, newValue);
        }

        private static Package PreLoadPackage(PackageSession session, ILogger log, string filePath, bool isSystemPackage, PackageCollection loadedPackages, PackageLoadParameters loadParameters)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (loadedPackages == null) throw new ArgumentNullException(nameof(loadedPackages));
            if (loadParameters == null) throw new ArgumentNullException(nameof(loadParameters));

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

                // Load the package without loading any assets
                var package = Package.LoadRaw(log, filePath);
                package.IsSystem = isSystemPackage;

                // Convert UPath to absolute (Package only)
                // Removed for now because it is called again in PackageSession.LoadAssembliesAndAssets (and running it twice result in dirty package)
                // If we remove it from here (and call it only in the other method), templates are not loaded (Because they are loaded via the package store that do not use PreLoadPackage)
                //if (loadParameters.ConvertUPathToAbsolute)
                //{
                //    var analysis = new PackageAnalysis(package, new PackageAnalysisParameters()
                //    {
                //        ConvertUPathTo = UPathType.Absolute,
                //        SetDirtyFlagOnAssetWhenFixingAbsoluteUFile = true,
                //        IsProcessingUPaths = true,
                //    });
                //    analysis.Run(log);
                //}
                // If the package doesn't have a meta name, fix it here (This is supposed to be done in the above disabled analysis - but we still need to do it!)
                if (string.IsNullOrWhiteSpace(package.Meta.Name) && package.FullPath != null)
                {
                    package.Meta.Name = package.FullPath.GetFileName();
                    package.IsDirty = true;
                }

                // Add the package has loaded before loading dependencies
                loadedPackages.Add(package);

                // Package has been loaded, register it in constraints so that we force each subsequent loads to use this one (or fails if version doesn't match)
                session.constraintProvider.AddConstraint(package.Meta.Name, new VersionSpec(package.Meta.Version.ToSemanticVersion()));

                // Load package dependencies
                // This will perform necessary asset upgrades
                // TODO: We should probably split package loading in two recursive top-level passes (right now those two passes are mixed, making it more difficult to make proper checks)
                //   - First, load raw packages with their dependencies recursively, then resolve dependencies and constraints (and print errors/warnings)
                //   - Then, if everything is OK, load the actual references and assets for each packages
                PreLoadPackageDependencies(session, log, package, loadedPackages, loadParameters);

                // Add the package to the session but don't freeze it yet
                session.Packages.Add(package);

                return package;
            }
            catch (Exception ex)
            {
                log.Error("Error while pre-loading package [{0}]", ex, filePath);
            }

            return null;
        }

        private static bool TryLoadAssets(PackageSession session, ILogger log, Package package, PackageLoadParameters loadParameters)
        {
            // Already loaded
            if (package.State >= PackageState.AssetsReady)
                return true;

            // Dependencies could not properly be loaded
            if (package.State < PackageState.DependenciesReady)
                return false;

            // A package upgrade has previously been tried and denied, so let's keep the package in this state
            if (package.State == PackageState.UpgradeFailed)
                return false;

            try
            {
                // First, check that dependencies have their assets loaded
                bool dependencyError = false;
                foreach (var dependency in package.FindDependencies(false, false))
                {
                    if (!TryLoadAssets(session, log, dependency, loadParameters))
                        dependencyError = true;
                }

                if (dependencyError)
                    return false;

                var pendingPackageUpgrades = new List<PendingPackageUpgrade>();

                // Note: Default state is upgrade failed (for early exit on error/exceptions)
                // We will update to success as soon as loading is finished.
                package.State = PackageState.UpgradeFailed;

                // Process store dependencies for upgraders
                foreach (var packageDependency in package.Meta.Dependencies)
                {
                    var dependencyPackage = session.Packages.Find(packageDependency);
                    if (dependencyPackage == null)
                    {
                        continue;
                    }

                    // Check for upgraders
                    var packageUpgrader = CheckPackageUpgrade(log, package, packageDependency, dependencyPackage);
                    if (packageUpgrader != null)
                    {
                        pendingPackageUpgrades.Add(new PendingPackageUpgrade(packageUpgrader, packageDependency, dependencyPackage));
                    }
                }

                // Prepare asset loading
                var newLoadParameters = loadParameters.Clone();
                newLoadParameters.AssemblyContainer = session.AssemblyContainer;

                // Default package version override
                newLoadParameters.ExtraCompileProperties = new Dictionary<string, string>();
                var defaultPackageOverride = NugetStore.GetPackageVersionVariable(PackageStore.Instance.DefaultPackageName) + "Override";
                var defaultPackageVersion = PackageStore.Instance.DefaultPackageVersion.Version;
                newLoadParameters.ExtraCompileProperties.Add(defaultPackageOverride, new Version(defaultPackageVersion.Major, defaultPackageVersion.Minor).ToString());
                if (loadParameters.ExtraCompileProperties != null)
                {
                    foreach (var property in loadParameters.ExtraCompileProperties)
                    {
                        newLoadParameters.ExtraCompileProperties[property.Key] = property.Value;
                    }
                }

                if (pendingPackageUpgrades.Count > 0)
                {
                    var upgradeAllowed = true;
                    // Need upgrades, let's ask user confirmation
                    if (loadParameters.PackageUpgradeRequested != null)
                    {
                        upgradeAllowed = loadParameters.PackageUpgradeRequested(package, pendingPackageUpgrades);
                    }

                    if (!upgradeAllowed)
                    {
                        log.Error("Necessary package migration for [{0}] has not been allowed", package.Meta.Name);
                        return false;
                    }

                    // Perform pre assembly load upgrade
                    foreach (var pendingPackageUpgrade in pendingPackageUpgrades)
                    {
                        var packageUpgrader = pendingPackageUpgrade.PackageUpgrader;
                        var dependencyPackage = pendingPackageUpgrade.DependencyPackage;
                        if (!packageUpgrader.UpgradeBeforeAssembliesLoaded(session, log, package, pendingPackageUpgrade.Dependency, dependencyPackage))
                        {
                            log.Error("Error while upgrading package [{0}] for [{1}] from version [{2}] to [{3}]", package.Meta.Name, dependencyPackage.Meta.Name, pendingPackageUpgrade.Dependency.Version, dependencyPackage.Meta.Version);
                            return false;
                        }
                    }
                }

                // Load assemblies. Set the package filename to the path on disk, in case of renaming.
                // TODO: Could referenced projects be associated to other packages than this one?
                newLoadParameters.ExtraCompileProperties.Add("SiliconStudioCurrentPackagePath", package.FullPath);
                package.LoadAssemblies(log, newLoadParameters);

                // Load list of assets
                newLoadParameters.AssetFiles = Package.ListAssetFiles(log, package, true, loadParameters.CancelToken);

                if (pendingPackageUpgrades.Count > 0)
                {
                    // Perform upgrades
                    foreach (var pendingPackageUpgrade in pendingPackageUpgrades)
                    {
                        var packageUpgrader = pendingPackageUpgrade.PackageUpgrader;
                        var dependencyPackage = pendingPackageUpgrade.DependencyPackage;
                        if (!packageUpgrader.Upgrade(session, log, package, pendingPackageUpgrade.Dependency, dependencyPackage, newLoadParameters.AssetFiles))
                        {
                            log.Error("Error while upgrading package [{0}] for [{1}] from version [{2}] to [{3}]", package.Meta.Name, dependencyPackage.Meta.Name, pendingPackageUpgrade.Dependency.Version, dependencyPackage.Meta.Version);
                            return false;
                        }

                        // Update dependency to reflect new requirement
                        pendingPackageUpgrade.Dependency.Version = pendingPackageUpgrade.PackageUpgrader.Attribute.PackageUpdatedVersionRange;
                    }

                    // Mark package as dirty
                    package.IsDirty = true;
                }

                // Load assets
                package.LoadAssets(log, newLoadParameters);

                // Validate assets from package
                package.ValidateAssets(newLoadParameters.GenerateNewAssetIds);

                if (pendingPackageUpgrades.Count > 0)
                {
                    // Perform post asset load upgrade
                    foreach (var pendingPackageUpgrade in pendingPackageUpgrades)
                    {
                        var packageUpgrader = pendingPackageUpgrade.PackageUpgrader;
                        var dependencyPackage = pendingPackageUpgrade.DependencyPackage;
                        if (!packageUpgrader.UpgradeAfterAssetsLoaded(session, log, package, pendingPackageUpgrade.Dependency, dependencyPackage, pendingPackageUpgrade.DependencyVersionBeforeUpgrade))
                        {
                            log.Error("Error while upgrading package [{0}] for [{1}] from version [{2}] to [{3}]", package.Meta.Name, dependencyPackage.Meta.Name, pendingPackageUpgrade.Dependency.Version, dependencyPackage.Meta.Version);
                            return false;
                        }
                    }

                    // Mark package as dirty
                    package.IsDirty = true;
                }

                // Mark package as ready
                package.State = PackageState.AssetsReady;

                // Freeze the package after loading the assets
                session.FreezePackage(package);

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error while loading package [{0}]", ex, package);
                return false;
            }
        }

        private static PackageUpgrader CheckPackageUpgrade(ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage)
        {
            // Don't do anything if source is a system (read-only) package for now
            // We only want to process local packages
            if (dependentPackage.IsSystem)
                return null;

            // Check if package might need upgrading
            var dependentPackagePreviousMinimumVersion = dependency.Version.MinVersion;
            if (dependentPackagePreviousMinimumVersion < dependencyPackage.Meta.Version)
            {
                // Find upgrader for given package
                // Note: If no upgrader is found, we assume it is still compatible with previous versions, so do nothing
                var packageUpgrader = AssetRegistry.GetPackageUpgrader(dependencyPackage.Meta.Name);
                if (packageUpgrader != null)
                {
                    // Check if upgrade is necessary
                    if (dependency.Version.MinVersion >= packageUpgrader.Attribute.PackageUpdatedVersionRange.MinVersion)
                    {
                        return null;
                    }

                    // Check if upgrade is allowed
                    if (dependency.Version.MinVersion < packageUpgrader.Attribute.PackageMinimumVersion)
                    {
                        // Throw an exception, because the package update is not allowed and can't be done
                        throw new InvalidOperationException($"Upgrading package [{dependentPackage.Meta.Name}] to use [{dependencyPackage.Meta.Name}] from version [{dependentPackagePreviousMinimumVersion}] to [{dependencyPackage.Meta.Version}] is not supported");
                    }

                    log.Info("Upgrading package [{0}] to use [{1}] from version [{2}] to [{3}] will be required", dependentPackage.Meta.Name, dependencyPackage.Meta.Name, dependentPackagePreviousMinimumVersion, dependencyPackage.Meta.Version);
                    return packageUpgrader;
                }
            }

            return null;
        }
        
        private static void PreLoadPackageDependencies(PackageSession session, ILogger log, Package package, PackageCollection loadedPackages, PackageLoadParameters loadParameters)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (loadParameters == null) throw new ArgumentNullException(nameof(loadParameters));

            bool packageDependencyErrors = false;

            // TODO: Remove and recheck Dependencies Ready if some secondary packages are removed?
            if (package.State >= PackageState.DependenciesReady)
                return;

            // 1. Load store package
            foreach (var packageDependency in package.Meta.Dependencies)
            {
                var loadedPackage = session.Packages.Find(packageDependency);
                if (loadedPackage == null)
                {
                    var file = PackageStore.Instance.GetPackageFileName(packageDependency.Name, packageDependency.Version, session.constraintProvider);

                    if (file == null)
                    {
                        // TODO: We need to support automatic download of packages. This is not supported yet when only Xenko
                        // package is supposed to be installed, but It will be required for full store
                        log.Error("Unable to find package {0} not installed", packageDependency);
                        packageDependencyErrors = true;
                        continue;
                    }

                    // Recursive load of the system package
                    loadedPackage = PreLoadPackage(session, log, file, true, loadedPackages, loadParameters);
                }

                if (loadedPackage == null || loadedPackage.State < PackageState.DependenciesReady)
                    packageDependencyErrors = true;
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
                var loadedPackage = PreLoadPackage(session, log, subPackageFilePath.FullPath, false, loadedPackages, loadParameters);

                if (loadedPackage == null || loadedPackage.State < PackageState.DependenciesReady)
                    packageDependencyErrors = true;
            }

            // 3. Update package state
            if (!packageDependencyErrors)
            {
                package.State = PackageState.DependenciesReady;
            }
        }

        public class PendingPackageUpgrade
        {
            public readonly PackageUpgrader PackageUpgrader;
            public readonly PackageDependency Dependency;
            public readonly Package DependencyPackage;
            public readonly PackageVersionRange DependencyVersionBeforeUpgrade;

            public PendingPackageUpgrade(PackageUpgrader packageUpgrader, PackageDependency dependency, Package dependencyPackage)
            {
                PackageUpgrader = packageUpgrader;
                Dependency = dependency;
                DependencyPackage = dependencyPackage;
                DependencyVersionBeforeUpgrade = Dependency.Version;
            }
        }

        private static PackageAnalysisParameters GetPackageAnalysisParametersForLoad()
        {
            return new PackageAnalysisParameters()
            {
                IsPackageCheckDependencies = true,
                IsProcessingAssetReferences = true,
                IsLoggingAssetNotFoundAsError = true,
                AssetTemplatingMergeModifiedAssets = true
            };
        }
    }
}