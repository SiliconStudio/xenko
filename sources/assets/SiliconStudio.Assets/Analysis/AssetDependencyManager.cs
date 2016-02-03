// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// A class responsible for providing asset dependencies for a <see cref="PackageSession"/> and file tracking dependency.
    /// </summary>
    /// <remarks>
    /// This class provides methods to:
    /// <ul>
    /// <li>Find assets referencing a particular asset (recursively or not)</li>
    /// <li>Find assets referenced by a particular asset (recursively or not)</li>
    /// <li>Find missing references</li>
    /// <li>Find missing references for a particular asset</li>
    /// <li>Find assets file changed events that have changed on the disk</li>
    /// </ul>
    /// </remarks>
    public sealed class AssetDependencyManager : IDisposable
    {
        private readonly PackageSession session;
        internal readonly object ThisLock = new object();
        internal readonly HashSet<Package> Packages;
        internal readonly Dictionary<Guid, AssetDependencies> Dependencies;
        internal readonly Dictionary<Guid, AssetDependencies> AssetsWithMissingReferences;
        internal readonly Dictionary<Guid, HashSet<AssetDependencies>> MissingReferencesToParent;

        // Objects used to track directories
        internal DirectoryWatcher DirectoryWatcher;
        private readonly Dictionary<Package, string> packagePathsTracked = new Dictionary<Package, string>();
        private readonly Dictionary<Guid, HashSet<UFile>> mapAssetToInputDependencies = new Dictionary<Guid, HashSet<UFile>>();
        private readonly Dictionary<string, HashSet<Guid>> mapInputDependencyToAssets = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Guid, ObjectId> mapAssetsToSource = new Dictionary<Guid, ObjectId>();
        private readonly List<FileEvent> fileEvents = new List<FileEvent>();
        private readonly List<FileEvent> fileEventsWorkingCopy = new List<FileEvent>();
        private readonly ManualResetEvent threadWatcherEvent;
        private readonly List<AssetFileChangedEvent> currentAssetFileChangedEvents = new List<AssetFileChangedEvent>();
        private readonly CancellationTokenSource tokenSourceForImportHash;
        private readonly List<AssetFileChangedEvent> sourceImportFileChangedEventsToAdd = new List<AssetFileChangedEvent>();
        private Thread fileEventThreadHandler;
        private int trackingSleepTime;
        private bool isDisposed;
        private bool isDisposing;
        private bool isTrackingPaused;
        private bool isSessionSaving;
        private readonly HashSet<string> assetsBeingSaved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly AssetFileChangedEventSquasher assetFileChangedEventSquasher = new AssetFileChangedEventSquasher();
        private bool isInitialized;
        //private Task initializingTask;

        /// <summary>
        /// Occurs when a asset changed. This event is called in the critical section of the dependency manager,
        /// meaning that dependencies can be safely computed via <see cref="ComputeDependencies"/> method from this callback.
        /// </summary>
        public event Action<AssetItem> AssetChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDependencyManager" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException">session</exception>
        internal AssetDependencyManager(PackageSession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.session = session;
            this.session.Packages.CollectionChanged += Packages_CollectionChanged;
            session.AssetDirtyChanged += Session_AssetDirtyChanged;
            AssetsWithMissingReferences = new Dictionary<Guid, AssetDependencies>();
            MissingReferencesToParent = new Dictionary<Guid, HashSet<AssetDependencies>>();
            Packages = new HashSet<Package>();
            Dependencies = new Dictionary<Guid, AssetDependencies>();
            TrackingSleepTime = 100;
            tokenSourceForImportHash = new CancellationTokenSource();
            threadWatcherEvent = new ManualResetEvent(false);

            // If the session has already a root package, then initialize the dependency manager directly
            if (session.LocalPackages.Any())
            {
                Initialize();
            }
        }

        //internal void InitializeDeferred()
        //{
        //    initializingTask = Task.Run(() => Initialize());
        //}

        /// <summary>
        /// Gets or sets a value indicating whether this instance should track file disk changed events. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if this instance should track file disk changed events; otherwise, <c>false</c>.</value>
        public bool EnableTracking
        {
            get
            {
                return fileEventThreadHandler != null;
            }
            set
            {
                if (isDisposed)
                {
                    throw new InvalidOperationException("Cannot enable tracking when this instance is disposed");
                }

                lock (ThisLock)
                {
                    if (value)
                    {
                        bool activateTracking = false;
                        if (DirectoryWatcher == null)
                        {
                            DirectoryWatcher = new DirectoryWatcher();
                            DirectoryWatcher.Modified += directoryWatcher_Modified;
                            activateTracking = true;
                        }

                        if (fileEventThreadHandler == null)
                        {
                            fileEventThreadHandler = new Thread(SafeAction.Wrap(RunChangeWatcher)) { IsBackground = true, Name = "RunChangeWatcher thread" };
                            fileEventThreadHandler.Start();
                        }

                        if (activateTracking)
                        {
                            ActivateTracking();
                        }
                    }
                    else 
                    {
                        if (DirectoryWatcher != null)
                        {
                            DirectoryWatcher.Dispose();
                            DirectoryWatcher = null;
                        }

                        if (fileEventThreadHandler != null)
                        {
                            threadWatcherEvent.Set();
                            fileEventThreadHandler.Join();
                            fileEventThreadHandler = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized. See remarks.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// If this instance is not initialized, all public methods may block until the full initialization of this instance.
        /// </remarks>
        public bool IsInitialized
        {
            get
            {
                return isInitialized;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is processing tracking events or it is paused. Default is <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if this instance is tracking paused; otherwise, <c>false</c>.</value>
        public bool IsTrackingPaused
        {
            get
            {
                return isTrackingPaused;
            }
            set
            {
                if (!EnableTracking)
                    return;

                isTrackingPaused = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of ms the file tracker should sleep before checking changes. Default is 1000ms.
        /// </summary>
        /// <value>The tracking sleep time.</value>
        public int TrackingSleepTime
        {
            get
            {
                return trackingSleepTime;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "TrackingSleepTime must be > 0");
                }
                trackingSleepTime = value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposing = true;
            tokenSourceForImportHash.Cancel();
            EnableTracking = false; // Will terminate the thread if running

            if (DirectoryWatcher != null)
            {
                DirectoryWatcher.Dispose();
            }
            //if (initializingTask != null)
            //{
            //    initializingTask.Wait();
            //}
            isDisposed = true;
        }

        /// <summary>
        /// Finds the changed events that have occurred, only valid if <see cref="EnableTracking"/> is set to <c>true</c>.
        /// </summary>
        /// <returns>List of events.</returns>
        public IEnumerable<AssetFileChangedEvent> FindAssetFileChangedEvents()
        {
            lock (Initialize())
            {
                var eventsCopy = assetFileChangedEventSquasher.Squash(currentAssetFileChangedEvents);
                currentAssetFileChangedEvents.Clear();
                return eventsCopy;
            }
        }

        /// <summary>
        /// Finds the dependencies for the specified asset.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <returns>The dependencies or null if not found.</returns>
        public AssetDependencies FindDependencySet(Guid assetId)
        {
            AssetDependencies dependencies;
            lock (Initialize())
            {
                if (Dependencies.TryGetValue(assetId, out dependencies))
                {
                    // Create a copy
                    dependencies = new AssetDependencies(dependencies);
                }
            }
            return dependencies;
        }

        /// <summary>
        /// Finds the assets inheriting from the specified asset id (this is a direct inheritance, not indirect).
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="searchOptions">The types of inheritance to search for</param>
        /// <returns>A list of asset inheriting from the specified asset id.</returns>
        public List<AssetItem> FindAssetsInheritingFrom(Guid assetId, AssetInheritanceSearchOptions searchOptions = AssetInheritanceSearchOptions.All)
        {
            var list = new List<AssetItem>();
            lock (Initialize())
            {
                ContentLinkType searchType = 0;
                if((searchOptions & AssetInheritanceSearchOptions.Base) != 0)
                    searchType |= ContentLinkType.Inheritance;
                if((searchOptions & AssetInheritanceSearchOptions.Composition) != 0)
                    searchType |= ContentLinkType.CompositionInheritance;

                AssetDependencies dependencies;
                if (Dependencies.TryGetValue(assetId, out dependencies))
                {
                    list.AddRange(dependencies.LinksIn.Where(p => (p.Type & searchType) != 0).Select(p => p.Item.Clone(true)));
                }
            }
            return list;
        }

        /// <summary>
        /// Finds the asset items by their input/import file.
        /// </summary>
        /// <param name="importFile">The import file.</param>
        /// <returns>A list of assets that are imported from the specified import file.</returns>
        /// <exception cref="System.ArgumentNullException">importFile</exception>
        public HashSet<Guid> FindAssetIdsByInput(string importFile)
        {
            if (importFile == null) throw new ArgumentNullException("importFile");
            lock (Initialize())
            {
                HashSet<Guid> assets;
                if (mapInputDependencyToAssets.TryGetValue(importFile, out assets))
                {
                    return new HashSet<Guid>(assets);
                }
                return new HashSet<Guid>();
            }
        }

        /// <summary>
        /// Finds the asset items by their input/import file.
        /// </summary>
        /// <param name="importFile">The import file.</param>
        /// <returns>A list of assets that are imported from the specified import file.</returns>
        /// <exception cref="System.ArgumentNullException">importFile</exception>
        public HashSet<AssetItem> FindAssetItemsByInput(string importFile)
        {
            if (importFile == null) throw new ArgumentNullException("importFile");
            lock (Initialize())
            {
                var ids = FindAssetIdsByInput(importFile);
                var items = new HashSet<AssetItem>(AssetItem.DefaultComparerById);
                foreach (var id in ids)
                {
                    items.Add(Dependencies[id].Item.Clone(true));
                }
                return items;
            }
        }

        /// <summary>
        /// Computes the dependencies for the specified asset.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <param name="dependenciesOptions">The dependencies options.</param>
        /// <param name="visited">The list of element already visited.</param>
        /// <returns>The dependencies.</returns>
        public AssetDependencies ComputeDependencies(AssetItem assetItem, AssetDependencySearchOptions dependenciesOptions = AssetDependencySearchOptions.All, HashSet<Guid> visited = null)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            bool recursive = (dependenciesOptions & AssetDependencySearchOptions.Recursive) != 0;
            if (visited == null && recursive)
                visited = new HashSet<Guid>();

            //var clock = Stopwatch.StartNew();

            lock (Initialize())
            {
                var dependencies = new AssetDependencies(assetItem);

                int inCount = 0, outCount = 0;

                if ((dependenciesOptions & AssetDependencySearchOptions.In) != 0)
                {
                    CollectInputReferences(dependencies, assetItem, visited, recursive, ref inCount);
                }

                if ((dependenciesOptions & AssetDependencySearchOptions.Out) != 0)
                {
                    if (visited != null)
                    {
                        visited.Clear();
                    }
                    CollectOutputReferences(dependencies, assetItem, visited, recursive, ref outCount);
                }

                //Console.WriteLine("Time to compute dependencies: {0}ms in: {1} out:{2}", clock.ElapsedMilliseconds, inCount, outCount);

                return dependencies;
            }

        }

        /// <summary>
        /// Gets a value indicating whether there is any missing references.
        /// </summary>
        /// <value><c>true</c> if this instance has missing references; otherwise, <c>false</c>.</value>
        public bool HasMissingReferences
        {
            get
            {
                lock (Initialize())
                {
                    return AssetsWithMissingReferences.Count > 0;
                }
            }
        }

        /// <summary>
        /// Finds the assets with missing references.
        /// </summary>
        /// <returns>An enumeration of asset guid that have missing references.</returns>
        public IEnumerable<Guid> FindAssetsWithMissingReferences()
        {
            lock (Initialize())
            {
                return AssetsWithMissingReferences.Keys.ToList();
            }
        }

        /// <summary>
        /// Finds the missing references for a particular asset.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <returns>IEnumerable{IContentReference}.</returns>
        /// <exception cref="System.ArgumentNullException">item</exception>
        public IEnumerable<IContentReference> FindMissingReferences(Guid assetId)
        {
            lock (Initialize())
            {
                AssetDependencies dependencies;
                if (AssetsWithMissingReferences.TryGetValue(assetId, out dependencies))
                {
                    return dependencies.BrokenLinksOut.Select(x => x.Element).ToList();
                }
            }

            return Enumerable.Empty<IContentReference>();
        }

        private object Initialize()
        {
            lock (ThisLock)
            {
                if (isInitialized)
                {
                    return ThisLock;
                }

                // If the package is cancelled, don't try to do anything
                // A cancellation means that the package session will be destroyed
                if (isDisposing)
                {
                    return ThisLock;
                }

                // Initialize with the list of packages
                foreach (var package in session.Packages)
                {
                    // If the package is cancelled, don't try to do anything
                    // A cancellation means that the package session will be destroyed
                    if (isDisposing)
                    {
                        return ThisLock;
                    }

                    TrackPackage(package);
                }

                isInitialized = true;
            }
            return ThisLock;
        }

        /// <summary>
        /// Collects all references of an asset dynamically.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="packageSession">The package session.</param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <param name="keepParents">Indicate if the parent of the provided <paramref name="result"/> should be kept or not</param>
        /// <exception cref="System.ArgumentNullException">packageSession</exception>
        private static void CollectDynamicOutReferences(AssetDependencies result, PackageSession packageSession, bool isRecursive, bool keepParents)
        {
            if (packageSession == null) throw new ArgumentNullException("packageSession");
            CollectDynamicOutReferences(result, packageSession.FindAsset, isRecursive, keepParents);
        }

        /// <summary>
        /// Collects all references of an asset dynamically.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="assetResolver">The asset resolver.</param>
        /// <param name="isRecursive">if set to <c>true</c> collects references recursively.</param>
        /// <param name="keepParents">Indicate if the parent of the provided <paramref name="result"/> should be kept or not</param>
        /// <exception cref="System.ArgumentNullException">
        /// result
        /// or
        /// assetResolver
        /// </exception>
        private static void CollectDynamicOutReferences(AssetDependencies result, Func<Guid, AssetItem> assetResolver, bool isRecursive, bool keepParents)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (assetResolver == null) throw new ArgumentNullException("assetResolver");

            var addedReferences = new HashSet<Guid>();
            var itemsToAnalyze = new Queue<AssetItem>();
            var referenceCollector = new DependenciesCollector();

            // Reset the dependencies/parts.
            result.Reset(keepParents);

            var assetItem = result.Item;

            // Collect part assets.
            var container = assetItem.Asset as IAssetComposite;
            if (container != null)
            {
                foreach (var part in container.CollectParts())
                {
                    result.AddPart(part);
                }
            }

            // marked as processed to not add it again
            addedReferences.Add(assetItem.Id);
            itemsToAnalyze.Enqueue(assetItem);

            while (itemsToAnalyze.Count > 0)
            {
                var item = itemsToAnalyze.Dequeue();

                foreach (var link in referenceCollector.GetDependencies(item))
                {
                    if (addedReferences.Contains(link.Element.Id))
                        continue;

                    // marked as processed to not add it again
                    addedReferences.Add(link.Element.Id);

                    // add the location to the reference location list
                    var nextItem = assetResolver(link.Element.Id);
                    if (nextItem != null)
                    {
                        result.AddLinkOut(nextItem, link.Type, false);

                        // add current element to analyze list, to analyze dependencies recursively
                        if (isRecursive)
                        {
                            itemsToAnalyze.Enqueue(nextItem);
                        }
                    }
                    else
                    {
                        result.AddBrokenLinkOut(link);
                    }
                }

                if (!isRecursive)
                {
                    break;
                }
            }
        }

        private AssetItem FindAssetFromDependencyOrSession(Guid guid)
        {
            // We cannot return the item from the session but we can only return assets currently tracked by the dependency 
            // manager
            var item = session.FindAsset(guid);
            if (item != null)
            {
                var dependencies = TrackAsset(guid);
                return dependencies.Item;
            }
            return null;
        }

        /// <summary>
        /// This methods is called when a session is about to being saved.
        /// </summary>
        public void BeginSavingSession()
        {
            isSessionSaving = true;
        }

        /// <summary>
        /// Adds the file being save during session save.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <exception cref="System.ArgumentNullException">file</exception>
        internal void AddFileBeingSaveDuringSessionSave(UFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!isSessionSaving) throw new InvalidOperationException("Cannot call this method outside a BeginSavingSession/EndSavingSession");

            lock (Initialize())
            {
                assetsBeingSaved.Add(file);
            }
        }

        /// <summary>
        /// This methods is called when a session has been saved.
        /// </summary>
        public void EndSavingSession()
        {
            isSessionSaving = false;

            // After saving, we must double-check that all packages are tracked correctly
            lock (Initialize())
            {
                foreach (var package in Packages)
                {
                    UpdatePackagePathTracked(package, true);
                }
            }
        }

        /// <summary>
        /// Calculate the dependencies for the specified asset either by using the internal cache if the asset is already in the session
        /// or by calculating 
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <returns>The dependencies.</returns>
        private AssetDependencies CalculateDependencies(AssetItem assetItem)
        {
            AssetDependencies dependencies;
            if (!Dependencies.TryGetValue(assetItem.Id, out dependencies))
            {
                // If the asset is not followed by this instance (this could not be part of the session)
                // We are allocating a new dependency on the fly and calculating first level dependencies
                dependencies = new AssetDependencies(assetItem);
                CollectDynamicOutReferences(dependencies, FindAssetFromDependencyOrSession, false, false);
            }
            return dependencies;
        }

        /// <summary>
        /// This method is called when a package needs to be tracked
        /// </summary>
        /// <param name="package">The package to track.</param>
        private void TrackPackage(Package package)
        {
            lock (ThisLock)
            {
                if (Packages.Contains(package))
                    return;

                Packages.Add(package);

                foreach (var asset in package.Assets)
                {
                    // If the package is cancelled, don't try to do anything
                    // A cancellation means that the package session will be destroyed
                    if (isDisposing)
                    {
                        return;
                    }

                    TrackAsset(asset);
                }

                package.Assets.CollectionChanged += Assets_CollectionChanged;
                UpdatePackagePathTracked(package, true);
            }
        }

        /// <summary>
        /// This method is called when a package needs to be un-tracked
        /// </summary>
        /// <param name="package">The package to un-track.</param>
        private void UnTrackPackage(Package package)
        {
            lock (ThisLock)
            {
                if (!Packages.Contains(package))
                    return;

                package.Assets.CollectionChanged -= Assets_CollectionChanged;

                foreach (var asset in package.Assets)
                {
                    UnTrackAsset(asset);
                }

                Packages.Remove(package);
                UpdatePackagePathTracked(package, false);
            }
        }

        /// <summary>
        /// This method is called when an asset needs to be tracked
        /// </summary>
        /// <param name="assetItemSource">The asset item source.</param>
        /// <returns>AssetDependencies.</returns>
        private AssetDependencies TrackAsset(AssetItem assetItemSource)
        {
            return TrackAsset(assetItemSource.Id);
        }

        /// <summary>
        /// This method is called when an asset needs to be tracked
        /// </summary>
        /// <returns>AssetDependencies.</returns>
        private AssetDependencies TrackAsset(Guid assetId)
        {
            lock (ThisLock)
            {
                AssetDependencies dependencies;
                if (Dependencies.TryGetValue(assetId, out dependencies))
                    return dependencies;

                // TODO provide an optimized version of TrackAsset method
                // taking directly a well known asset (loaded from a Package...etc.)
                // to avoid session.FindAsset 
                var assetItem = session.FindAsset(assetId);
                if (assetItem == null)
                {
                    return null;
                }

                // Clone the asset before using it in this instance to make sure that
                // we have some kind of immutable state
                // TODO: This is not handling shadow registry

                // No need to clone assets from readonly package 
                var assetItemCloned = assetItem.Package.IsSystem
                    ? assetItem
                    : new AssetItem(assetItem.Location, (Asset)AssetCloner.Clone(assetItem.Asset), assetItem.Package)
                        {
                            SourceFolder = assetItem.SourceFolder,
                            SourceProject = assetItem.SourceProject
                        };
                
                dependencies = new AssetDependencies(assetItemCloned);

                // Adds to global list
                Dependencies.Add(assetId, dependencies);

                // Update dependencies
                UpdateAssetDependencies(dependencies);
                CheckAllDependencies();

                return dependencies;
            }
        }

        private void CheckAllDependencies()
        {
            //foreach (var dependencies in Dependencies.Values)
            //{
            //    foreach (var outDependencies in dependencies)
            //    {
            //        if (outDependencies.Package == null)
            //        {
            //            System.Diagnostics.Debugger.Break();
            //        }
            //    }
            //}
        }

        /// <summary>
        /// This method is called when an asset needs to be un-tracked
        /// </summary>
        /// <param name="assetItemSource">The asset item source.</param>
        private void UnTrackAsset(AssetItem assetItemSource)
        {
            lock (ThisLock)
            {
                var assetId = assetItemSource.Id;
                AssetDependencies dependencies;
                if (!Dependencies.TryGetValue(assetId, out dependencies))
                    return;

                // Remove from global list
                Dependencies.Remove(assetId);

                // Remove previous missing dependencies
                RemoveMissingDependencies(dependencies);

                // Update [In] dependencies for children
                foreach (var childItem in dependencies.LinksOut)
                {
                    AssetDependencies childDependencyItem;
                    if (Dependencies.TryGetValue(childItem.Item.Id, out childDependencyItem))
                    {
                        childDependencyItem.RemoveLinkIn(dependencies.Item);
                    }
                }

                // Update [Out] dependencies for parents
                foreach (var parentDependencies in dependencies.LinksIn)
                {
                    var assetDependencies = Dependencies[parentDependencies.Item.Id];
                    var linkOut = assetDependencies.RemoveLinkOut(dependencies.Item);
                    assetDependencies.AddBrokenLinkOut(linkOut);

                    UpdateMissingDependencies(assetDependencies);
                }

                // Track asset import paths
                UpdateAssetImportPathsTracked(dependencies.Item, false);
            }

            CheckAllDependencies();
        }

        private void UpdateAssetDependencies(AssetDependencies dependencies)
        {
            lock (ThisLock)
            {
                // Track asset import paths
                UpdateAssetImportPathsTracked(dependencies.Item, true);

                // Remove previous part assets registered
                foreach (var part in dependencies.Parts)
                {
                    Dependencies.Remove(part.Id);
                }

                // Remove previous missing dependencies
                RemoveMissingDependencies(dependencies);

                // Remove [In] dependencies from previous children
                foreach (var referenceAsset in dependencies.LinksOut)
                {
                    var childDependencyItem = TrackAsset(referenceAsset.Item);
                    if (childDependencyItem != null)
                    {
                        childDependencyItem.RemoveLinkIn(dependencies.Item);
                    }
                }

                // Recalculate [Out] dependencies
                CollectDynamicOutReferences(dependencies, FindAssetFromDependencyOrSession, false, true);

                // Add part assets
                foreach (var part in dependencies.Parts)
                {
                    Dependencies[part.Id] = dependencies;
                }

                // Add [In] dependencies to new children
                foreach (var assetLink in dependencies.LinksOut)
                {
                    var childDependencyItem = TrackAsset(assetLink.Item);
                    if (childDependencyItem != null)
                    {
                        childDependencyItem.AddLinkIn(dependencies.Item, assetLink.Type, false);
                    }
                }

                // Update missing dependencies
                UpdateMissingDependencies(dependencies);
            }
        }

        private void RemoveMissingDependencies(AssetDependencies dependencies)
        {
            if (AssetsWithMissingReferences.ContainsKey(dependencies.Item.Id))
            {
                AssetsWithMissingReferences.Remove(dependencies.Item.Id);
                foreach (var assetLink in dependencies.BrokenLinksOut)
                {
                    var list = MissingReferencesToParent[assetLink.Element.Id];
                    list.Remove(dependencies);
                    if (list.Count == 0)
                    {
                        MissingReferencesToParent.Remove(assetLink.Element.Id);
                    }
                }
            }
        }

        private void UpdateMissingDependencies(AssetDependencies dependencies)
        {
            HashSet<AssetDependencies> parentDependencyItems;
            // If the asset has any missing dependencies, update the fast lookup tables
            if (dependencies.HasMissingDependencies)
            {
                AssetsWithMissingReferences[dependencies.Item.Id] = dependencies;

                foreach (var assetLink in dependencies.BrokenLinksOut)
                {
                    if (!MissingReferencesToParent.TryGetValue(assetLink.Element.Id, out parentDependencyItems))
                    {
                        parentDependencyItems = new HashSet<AssetDependencies>();
                        MissingReferencesToParent.Add(assetLink.Element.Id, parentDependencyItems);
                    }

                    parentDependencyItems.Add(dependencies);
                }
            }

            var item = dependencies.Item;

            // If the new asset was a missing reference, remove all missing references for this asset
            if (MissingReferencesToParent.TryGetValue(item.Id, out parentDependencyItems))
            {
                MissingReferencesToParent.Remove(item.Id);
                foreach (var parentDependencies in parentDependencyItems)
                {
                    // Remove missing dependency from parent
                    var oldBrokenLink = parentDependencies.RemoveBrokenLinkOut(item.Id);

                    // Update [Out] dependency to parent
                    parentDependencies.AddLinkOut(item, oldBrokenLink.Type, false);

                    // Update [In] dependency to current
                    dependencies.AddLinkIn(parentDependencies.Item, oldBrokenLink.Type, false);

                    // Remove global cache for assets with missing references
                    if (!parentDependencies.HasMissingDependencies)
                    {
                        AssetsWithMissingReferences.Remove(parentDependencies.Item.Id);
                    }
                }
            }
        }

        private void UpdatePackagePathTracked(Package package, bool isTracking)
        {
            // Don't try to track system package
            if (package.IsSystem)
            {
                return;
            }

            lock (ThisLock)
            {
                if (isTracking)
                {
                    string previousLocation;
                    packagePathsTracked.TryGetValue(package, out previousLocation);

                    string newLocation = package.RootDirectory;
                    bool trackNewLocation = newLocation != null && Directory.Exists(newLocation);
                    if (previousLocation != null)
                    {
                        bool unTrackPreviousLocation = false;
                        // If the package has no longer any directory, we have to only remove it from previous tracked
                        if (package.RootDirectory == null)
                        {
                            unTrackPreviousLocation = true;
                            trackNewLocation = false;
                        }
                        else
                        {
                            newLocation = package.RootDirectory;
                            if (string.Compare(previousLocation, newLocation, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                unTrackPreviousLocation = true;
                            }
                            else
                            {
                                // Nothing to do, previous location is the same
                                trackNewLocation = false;
                            }
                        }

                        // Untrack the previous location if different
                        if (unTrackPreviousLocation)
                        {
                            packagePathsTracked.Remove(package);
                            if (DirectoryWatcher != null)
                            {
                                DirectoryWatcher.UnTrack(previousLocation);
                            }
                        }
                    }

                    if (trackNewLocation)
                    {
                        // Track the location
                        packagePathsTracked[package] = newLocation;
                        if (DirectoryWatcher != null)
                        {
                            DirectoryWatcher.Track(newLocation);
                        }
                    }
                }
                else
                {
                    string previousLocation;
                    if (packagePathsTracked.TryGetValue(package, out previousLocation))
                    {
                        // Untrack the previous location
                        if (DirectoryWatcher != null)
                        {
                            DirectoryWatcher.UnTrack(previousLocation);
                        }
                        packagePathsTracked.Remove(package);
                    }
                }
            }
        }

        private void TrackAssetImportInput(AssetItem assetItem, string inputPath)
        {
            lock (ThisLock)
            {
                HashSet<Guid> assetsTrackedByPath;
                if (!mapInputDependencyToAssets.TryGetValue(inputPath, out assetsTrackedByPath))
                {
                    assetsTrackedByPath = new HashSet<Guid>();
                    mapInputDependencyToAssets.Add(inputPath, assetsTrackedByPath);
                    if (DirectoryWatcher != null)
                    {
                        DirectoryWatcher.Track(inputPath);
                    }
                }
                assetsTrackedByPath.Add(assetItem.Id);
            }

            // We will always issue a compute of the hash in order to verify SourceHash haven't changed
            FileVersionManager.Instance.ComputeFileHashAsync(inputPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
        }

        private void ActivateTracking()
        {
            List<string> files;
            lock (ThisLock)
            {
                files = mapInputDependencyToAssets.Keys.ToList();
            }
            foreach (var inputPath in files)
            {
                DirectoryWatcher.Track(inputPath);
                FileVersionManager.Instance.ComputeFileHashAsync(inputPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
            }
        }

        private void UnTrackAssetImportInput(AssetItem assetItem, string inputPath)
        {
            lock (ThisLock)
            {
                HashSet<Guid> assetsTrackedByPath;
                if (mapInputDependencyToAssets.TryGetValue(inputPath, out assetsTrackedByPath))
                {
                    assetsTrackedByPath.Remove(assetItem.Id);
                    if (assetsTrackedByPath.Count == 0)
                    {
                        mapInputDependencyToAssets.Remove(inputPath);
                        if (DirectoryWatcher != null)
                        {
                            DirectoryWatcher.UnTrack(inputPath);
                        }
                    }
                }
            }
        }

        private void UpdateAssetImportPathsTracked(AssetItem assetItem, bool isTracking)
        {
            // Only handle AssetImport
            var assetImport = assetItem.Asset as AssetImport;
            if (assetImport == null)
            {
                return;
            }

            if (isTracking)
            {
                // Currently an AssetImport is linked only to a single entry, but it could have probably have multiple input dependencies in the future
                var newInputPathDependencies = new HashSet<UFile>();
                var pathToSourceRawAsset = assetImport.Source;
                if (string.IsNullOrEmpty(pathToSourceRawAsset))
                {
                    return;
                }
                if (!pathToSourceRawAsset.IsAbsolute)
                {
                    pathToSourceRawAsset = UPath.Combine(assetItem.FullPath.GetParent(), pathToSourceRawAsset);
                }

                newInputPathDependencies.Add(pathToSourceRawAsset);

                HashSet<UFile> inputPaths;
                if (mapAssetToInputDependencies.TryGetValue(assetItem.Id, out inputPaths))
                {
                    // Untrack previous paths
                    foreach (var inputPath in inputPaths)
                    {
                        if (!newInputPathDependencies.Contains(inputPath))
                        {
                            UnTrackAssetImportInput(assetItem, inputPath);
                        }
                    }

                    // Track new paths
                    foreach (var inputPath in newInputPathDependencies)
                    {
                        if (!inputPaths.Contains(inputPath))
                        {
                            TrackAssetImportInput(assetItem, inputPath);
                        }
                    }
                }
                else
                {
                    // Track new paths
                    foreach (var inputPath in newInputPathDependencies)
                    {
                        TrackAssetImportInput(assetItem, inputPath);
                    }
                }

                mapAssetToInputDependencies[assetItem.Id] = newInputPathDependencies;
            }
            else
            {
                HashSet<UFile> inputPaths;
                if (mapAssetToInputDependencies.TryGetValue(assetItem.Id, out inputPaths))
                {
                    mapAssetToInputDependencies.Remove(assetItem.Id);
                    foreach (var inputPath in inputPaths)
                    {
                        UnTrackAssetImportInput(assetItem, inputPath);
                    }
                }
            }
        }

        private void directoryWatcher_Modified(object sender, FileEvent e)
        {
            // If tracking is not enabled, don't bother to track files on disk
            if (!EnableTracking)
                return;

            // Store only the most recent events
            lock (fileEvents)
            {
                fileEvents.Add(e);
            }
        }

        private void Session_AssetDirtyChanged(Asset asset)
        {
            // Don't update assets while saving
            // This is to avoid updating the dependency manager when saving an asset
            // TODO: We should handle assets modification while saving differently
            if (isSessionSaving)
            {
                return;
            }

            lock (ThisLock)
            {
                AssetDependencies dependencies;
                if (Dependencies.TryGetValue(asset.Id, out dependencies))
                {
                    dependencies.Item.Asset = (Asset)AssetCloner.Clone(asset);
                    UpdateAssetDependencies(dependencies);

                    // Notify an asset changed
                    OnAssetChanged(dependencies.Item);
                }
                else
                {
                    var package = asset as Package;
                    if (package != null)
                    {
                        UpdatePackagePathTracked(package, true);
                    }
                }
            }

            CheckAllDependencies();
        }

        private void Packages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    TrackPackage((Package)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnTrackPackage((Package)e.OldItems[0]);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var oldPackage in e.OldItems.OfType<Package>())
                    {
                        UnTrackPackage(oldPackage);
                    }

                    foreach (var packageToCopy in session.Packages)
                    {
                        TrackPackage(packageToCopy);
                    }
                    break;
            }
        }
        
        private void Assets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    TrackAsset(((AssetItem)e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnTrackAsset(((AssetItem)e.OldItems[0]));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    var collection = (PackageAssetCollection)sender;

                    var items = Dependencies.Values.Where(item => ReferenceEquals(item.Item.Package, collection.Package)).ToList();
                    foreach (var assetItem in items)
                    {
                        UnTrackAsset(assetItem.Item);
                    }
                    foreach (var assetItem in collection)
                    {
                        TrackAsset(assetItem);
                    }
                    break;
            }
        }

        /// <summary>
        /// This method is running in a separate thread and process file events received from <see cref="Core.IO.DirectoryWatcher"/>
        /// in order to generate the appropriate list of <see cref="AssetFileChangedEvent"/>.
        /// </summary>
        private void RunChangeWatcher()
        {
            // Only check every minute
            while (true)
            {
                if (threadWatcherEvent.WaitOne(TrackingSleepTime))
                    break;

                // Use a working copy in order to limit the locking
                fileEventsWorkingCopy.Clear();
                lock (fileEvents)
                {
                    fileEventsWorkingCopy.AddRange(fileEvents);
                    fileEvents.Clear();
                }

                if (fileEventsWorkingCopy.Count == 0 || isTrackingPaused)
                    continue;

                var assetEvents = new List<AssetFileChangedEvent>();

                // If this an asset belonging to a package
                lock (ThisLock)
                {
                    var packages = Packages;

                    // File event
                    foreach (var fileEvent in fileEventsWorkingCopy)
                    {
                        var file = new UFile(fileEvent.FullPath);

                        // When the session is being saved, we should not process events are they are false-positive alerts
                        // So we just skip the file
                        if (assetsBeingSaved.Contains(file.FullPath))
                        {
                            continue;
                        }

                        // 1) Check if this is related to imported assets
                        // Asset imports are handled slightly differently as we need to compute the
                        // hash of the source file
                        if (mapInputDependencyToAssets.ContainsKey(file.FullPath))
                        {
                            // Prepare the hash of the import file in advance for later re-import
                            FileVersionManager.Instance.ComputeFileHashAsync(file.FullPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
                            continue;
                        }

                        // 2) else check that the file is a supported extension
                        if (!AssetRegistry.IsAssetFileExtension(file.GetFileExtension()))
                        {
                            continue;
                        }

                        // Find the parent package of the file that has been updated
                        UDirectory parentPackagePath = null;
                        Package parentPackage = null;
                        foreach (var package in packages)
                        {
                            var rootDirectory = package.RootDirectory;
                            if (rootDirectory == null)
                                continue;

                            if (rootDirectory.Contains(file) && (parentPackagePath == null || parentPackagePath.FullPath.Length < rootDirectory.FullPath.Length))
                            {
                                parentPackagePath = rootDirectory;
                                parentPackage = package;
                            }
                        }

                        // If we found a parent package, create an associated asset event
                        if (parentPackage != null)
                        {
                            var relativeLocation = file.MakeRelative(parentPackagePath);

                            var item = parentPackage.Assets.Find(relativeLocation);
                            AssetFileChangedEvent evt = null;
                            switch (fileEvent.ChangeType)
                            {
                                case FileEventChangeType.Created:
                                    evt = new AssetFileChangedEvent(parentPackage, AssetFileChangedType.Added, relativeLocation);
                                    break;
                                case FileEventChangeType.Deleted:
                                    evt = new AssetFileChangedEvent(parentPackage, AssetFileChangedType.Deleted, relativeLocation);
                                    break;
                                case FileEventChangeType.Changed:
                                    evt = new AssetFileChangedEvent(parentPackage, AssetFileChangedType.Updated, relativeLocation);
                                    break;
                            }
                            if (evt != null)
                            {
                                if (item != null)
                                {
                                    evt.AssetId = item.Id;
                                }
                                assetEvents.Add(evt);
                            }
                        }
                    }


                    // After all events have been processed, we 
                    // remove the file assetBeingSaved
                    foreach (var fileEvent in fileEventsWorkingCopy)
                    {
                        var file = new UFile(fileEvent.FullPath);

                        // When the session is being saved, we should not process events are they are false-positive alerts
                        // So we just skip the file
                        if (assetsBeingSaved.Remove(file))
                        {
                            if (assetsBeingSaved.Count == 0)
                            {
                                break;
                            }
                        }
                    }


                    // If we have any new events, copy them back
                    if (assetEvents.Count > 0 && !isTrackingPaused)
                    {
                        currentAssetFileChangedEvents.AddRange(assetEvents);
                    }
                }
            }
        }

        /// <summary>
        /// This callback is receiving hash calculated from asset source file. If the source hash is changing from what
        /// we had previously stored, we can emit a <see cref="AssetFileChangedType.SourceUpdated" /> event.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="hash">The object identifier hash calculated from this source file.</param>
        private void SourceImportFileHashCallback(UFile sourceFile, ObjectId hash)
        {
            lock (ThisLock)
            {
                HashSet<Guid> items;
                if (!mapInputDependencyToAssets.TryGetValue(sourceFile, out items))
                {
                    return;
                }
                foreach (var itemId in items)
                {
                    AssetDependencies dependencies;
                    Dependencies.TryGetValue(itemId, out dependencies);
                    if (dependencies == null)
                    {
                        continue;
                    }

                    var item = dependencies.Item;

                    bool shouldNotifyChange;
                    var assetImport = item.Asset as AssetImportTracked;

                    if (assetImport != null)
                    {
                        shouldNotifyChange = assetImport.Base != null && assetImport.Base.IsRootImport && assetImport.SourceHash != hash;
                    }
                    else
                    {
                         ObjectId previousHash;
                         shouldNotifyChange = !mapAssetsToSource.TryGetValue(item.Id, out previousHash) || previousHash != hash;
                         mapAssetsToSource[item.Id] = hash;
                    }

                    if (shouldNotifyChange)
                    {
                        // If the hash is empty, the source file has been deleted
                        var changeType = (hash == ObjectId.Empty) ? AssetFileChangedType.SourceDeleted : AssetFileChangedType.SourceUpdated;

                        // Transmit the hash in the event as well, so that we can check again if the asset has not been updated during the async round-trip
                        // (it happens when re-importing multiple assets at once).
                        sourceImportFileChangedEventsToAdd.Add(new AssetFileChangedEvent(item.Package, changeType, item.Location) { AssetId = item.Id, Hash = hash });
                    }
                }

                if (sourceImportFileChangedEventsToAdd.Count > 0 && !isTrackingPaused)
                {
                    currentAssetFileChangedEvents.AddRange(sourceImportFileChangedEventsToAdd);
                }
                sourceImportFileChangedEventsToAdd.Clear();
            }
        }

        private void CollectInputReferences(AssetDependencies dependencyRoot, AssetItem assetItem, HashSet<Guid> visited, bool recursive, ref int count)
        {
            var assetId = assetItem.Id;
            if (visited != null)
            {
                if (visited.Contains(assetId))
                    return;

                visited.Add(assetId);
            }

            count++;

            AssetDependencies dependencies;
            Dependencies.TryGetValue(assetId, out dependencies);
            if (dependencies != null)
            {
                foreach (var pair in dependencies.LinksIn)
                {
                    dependencyRoot.AddLinkIn(pair, true);

                    if (visited != null && recursive)
                    {
                        CollectInputReferences(dependencyRoot, pair.Item, visited, true, ref count);
                    }
                }
            }
        }

        private void CollectOutputReferences(AssetDependencies dependencyRoot, AssetItem assetItem, HashSet<Guid> visited, bool recursive, ref int count)
        {
            var assetId = assetItem.Id;
            if (visited != null)
            {
                if (visited.Contains(assetId))
                    return;

                visited.Add(assetId);
            }

            count++;

            var dependencies = CalculateDependencies(assetItem);

            // Add missing references
            foreach (var missingRef in dependencies.BrokenLinksOut)
            {
                dependencyRoot.AddBrokenLinkOut(missingRef);
            }

            // Add output references
            foreach (var child in dependencies.LinksOut)
            {
                dependencyRoot.AddLinkOut(child, true);

                if (visited != null && recursive)
                {
                    CollectOutputReferences(dependencyRoot, child.Item, visited, true, ref count);
                }
            }
        }

        /// <summary>
        /// An interface providing methods to collect of asset references from an <see cref="AssetItem"/>.
        /// </summary>
        private interface IDependenciesCollector
        {
            /// <summary>
            /// Get the asset references of an <see cref="AssetItem"/>. This function is not recursive.
            /// </summary>
            /// <param name="item">The item we when the references of</param>
            /// <returns></returns>
            IEnumerable<IContentLink> GetDependencies(AssetItem item);
        }

        /// <summary>
        /// Visitor that collect all asset references.
        /// </summary>
        private class DependenciesCollector : AssetVisitorBase, IDependenciesCollector
        {
            private AssetDependencies dependencies;

            public IEnumerable<IContentLink> GetDependencies(AssetItem item)
            {
                dependencies = new AssetDependencies(item);

                Visit(item.Asset);
                
                // composition inheritances
                if (item.Asset.BaseParts != null)
                {
                    foreach (var compositionBase in item.Asset.BaseParts)
                        dependencies.AddBrokenLinkOut(compositionBase, ContentLinkType.CompositionInheritance);
                }

                return dependencies.BrokenLinksOut;
            }

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                // references and base
                var reference = obj as IContentReference;
                if (reference == null)
                {
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(obj);
                    if (attachedReference != null && attachedReference.IsProxy)
                        reference = attachedReference;
                }

                if (reference != null)
                {
                    var isBase = reference is AssetBase;

                    // Don't record base import
                    if (isBase && ((AssetBase)reference).IsRootImport)
                        return;

                    dependencies.AddBrokenLinkOut(reference, (isBase ? ContentLinkType.Inheritance: 0) | ContentLinkType.Reference);
                }
                else
                {
                    base.VisitObject(obj, descriptor, visitMembers);
                }
            }

            public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
            {
                // Don't visit base parts as they are visited at the top level.
                if (typeof(Asset).IsAssignableFrom(member.DeclaringType) && (member.Name == Asset.BasePartsProperty))
                {
                    return;
                }

                base.VisitObjectMember(container, containerDescriptor, member, value);
            }
        }

        private void OnAssetChanged(AssetItem obj)
        {
            Action<AssetItem> handler = AssetChanged;
            // Make sure we clone the item here only if it is necessary
            // Cloning the AssetItem is mandatory in order to make sure
            // the asset item won't change
            if (handler != null) handler(obj.Clone(true));
        }
    }
}