using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
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
    public sealed class AssetSourceTracker : IDisposable
    {
        private readonly PackageSession session;
        internal readonly object ThisLock = new object();
        internal readonly HashSet<Package> Packages;
        internal readonly Dictionary<Guid, AssetItem> TrackedAssets = new Dictionary<Guid, AssetItem>();

        // Objects used to track directories
        internal DirectoryWatcher DirectoryWatcher;
        private readonly Dictionary<Guid, Dictionary<UFile, bool>> mapAssetToInputDependencies = new Dictionary<Guid, Dictionary<UFile, bool>>();
        private readonly Dictionary<string, HashSet<Guid>> mapInputDependencyToAssets = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<FileEvent> fileEvents = new List<FileEvent>();
        private readonly List<FileEvent> fileEventsWorkingCopy = new List<FileEvent>();
        private readonly ManualResetEvent threadWatcherEvent;
        private readonly List<AssetFileChangedEvent> currentAssetFileChangedEvents = new List<AssetFileChangedEvent>();
        private readonly CancellationTokenSource tokenSourceForImportHash;
        private Thread fileEventThreadHandler;
        private int trackingSleepTime;
        private bool isDisposed;
        private bool isDisposing;
        private bool isTrackingPaused;
        private bool isInitialized;
        private readonly AssetFileChangedEventSquasher assetFileChangedEventSquasher = new AssetFileChangedEventSquasher();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDependencyManager" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException">session</exception>
        internal AssetSourceTracker(PackageSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            this.session = session;
            this.session.Packages.CollectionChanged += Packages_CollectionChanged;
            session.AssetDirtyChanged += Session_AssetDirtyChanged;
            Packages = new HashSet<Package>();
            TrackingSleepTime = 100;
            tokenSourceForImportHash = new CancellationTokenSource();
            threadWatcherEvent = new ManualResetEvent(false);

            // If the session has already a root package, then initialize the dependency manager directly
            if (session.LocalPackages.Any())
            {
                Initialize();
            }
        }


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
        public bool IsInitialized => isInitialized;

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
                    throw new ArgumentOutOfRangeException(nameof(value), @"TrackingSleepTime must be > 0");
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

            DirectoryWatcher?.Dispose();
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
        /// Finds the asset items by their input/import file.
        /// </summary>
        /// <param name="importFile">The import file.</param>
        /// <returns>A list of assets that are imported from the specified import file.</returns>
        /// <exception cref="System.ArgumentNullException">importFile</exception>
        [Obsolete]
        public HashSet<AssetItem> FindAssetItemsByInput(string importFile)
        {
            //if (importFile == null) throw new ArgumentNullException(nameof(importFile));
            lock (Initialize())
            {
                //var ids = FindAssetIdsByInput(importFile);
                var items = new HashSet<AssetItem>(AssetItem.DefaultComparerById);
                //foreach (var id in ids)
                //{
                //    items.Add(TrackedAssets[id].Clone(true));
                //}
                return items;
            }
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
            }
        }

        /// <summary>
        /// This method is called when an asset needs to be tracked
        /// </summary>
        /// <param name="assetItemSource">The asset item source.</param>
        /// <returns>AssetDependencies.</returns>
        private void TrackAsset(AssetItem assetItemSource)
        {
            TrackAsset(assetItemSource.Id);
        }

        /// <summary>
        /// This method is called when an asset needs to be tracked
        /// </summary>
        /// <returns>AssetDependencies.</returns>
        private void TrackAsset(Guid assetId)
        {
            lock (ThisLock)
            {
                if (TrackedAssets.ContainsKey(assetId))
                    return;

                // TODO provide an optimized version of TrackAsset method
                // taking directly a well known asset (loaded from a Package...etc.)
                // to avoid session.FindAsset 
                var assetItem = session.FindAsset(assetId);
                if (assetItem == null)
                    return;

                // Clone the asset before using it in this instance to make sure that
                // we have some kind of immutable state
                // TODO: This is not handling shadow registry

                // No need to clone assets from readonly package 
                var assetItemCloned = assetItem.Package.IsSystem
                    ? assetItem
                    : new AssetItem(assetItem.Location, (Asset)AssetCloner.Clone(assetItem.Asset, AssetClonerFlags.KeepBases), assetItem.Package)
                    {
                        SourceFolder = assetItem.SourceFolder,
                        SourceProject = assetItem.SourceProject
                    };

                // Adds to global list
                TrackedAssets.Add(assetId, assetItemCloned);

                // Update dependencies
                UpdateAssetImportPathsTracked(assetItemCloned, true);
            }
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
                AssetItem assetItem;
                if (!TrackedAssets.TryGetValue(assetId, out assetItem))
                    return;

                // Remove from global list
                TrackedAssets.Remove(assetId);

                // Track asset import paths
                UpdateAssetImportPathsTracked(assetItem, false);
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
                    DirectoryWatcher?.Track(inputPath);
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
                        DirectoryWatcher?.UnTrack(inputPath);
                    }
                }
            }
        }

        private void UpdateAssetImportPathsTracked(AssetItem assetItem, bool isTracking)
        {
            Dictionary<UFile, bool> oldSourceFiles;
            if (isTracking)
            {
                var collector = new SourceFilesCollector();
                var newSourceFiles = collector.GetSourceFiles(assetItem);
                //newSourceFiles.Add(pathToSourceRawAsset);

                if (mapAssetToInputDependencies.TryGetValue(assetItem.Id, out oldSourceFiles))
                {
                    // Untrack previous paths
                    foreach (var sourceFile in oldSourceFiles.Keys)
                    {
                        if (!newSourceFiles.ContainsKey(sourceFile))
                        {
                            UnTrackAssetImportInput(assetItem, sourceFile);
                        }
                    }

                    // Track new paths
                    foreach (var sourceFile in newSourceFiles.Keys)
                    {
                        if (!oldSourceFiles.ContainsKey(sourceFile))
                        {
                            TrackAssetImportInput(assetItem, sourceFile);
                        }
                    }
                }
                else
                {
                    // Track new paths
                    foreach (var sourceFile in newSourceFiles.Keys)
                    {
                        TrackAssetImportInput(assetItem, sourceFile);
                    }
                }

                mapAssetToInputDependencies[assetItem.Id] = newSourceFiles;
            }
            else
            {
                if (mapAssetToInputDependencies.TryGetValue(assetItem.Id, out oldSourceFiles))
                {
                    mapAssetToInputDependencies.Remove(assetItem.Id);
                    foreach (var sourceFile in oldSourceFiles.Keys)
                    {
                        UnTrackAssetImportInput(assetItem, sourceFile);
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

        private void Session_AssetDirtyChanged(Asset asset, bool oldValue, bool newValue)
        {
            // Don't update the source tracker while saving (setting dirty flag to false)
            if (newValue)
            {
                lock (ThisLock)
                {
                    AssetItem assetItem;
                    if (TrackedAssets.TryGetValue(asset.Id, out assetItem))
                    {
                        assetItem.Asset = (Asset)AssetCloner.Clone(asset, AssetClonerFlags.KeepBases);
                        UpdateAssetImportPathsTracked(assetItem, true);
                    }
                }
            }
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

                    foreach (var package in session.Packages)
                    {
                        TrackPackage(package);
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

                    var items = TrackedAssets.Values.Where(item => ReferenceEquals(item.Package, collection.Package)).ToList();
                    foreach (var assetItem in items)
                    {
                        UnTrackAsset(assetItem);
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
            while (!threadWatcherEvent.WaitOne(TrackingSleepTime))
            {
                // Use a working copy in order to limit the locking
                fileEventsWorkingCopy.Clear();
                lock (fileEvents)
                {
                    fileEventsWorkingCopy.AddRange(fileEvents);
                    fileEvents.Clear();
                }

                if (fileEventsWorkingCopy.Count == 0 || isTrackingPaused)
                    continue;

                // If this an asset belonging to a package
                lock (ThisLock)
                {
                    // File event
                    foreach (var fileEvent in fileEventsWorkingCopy)
                    {
                        var file = new UFile(fileEvent.FullPath);
                        if (mapInputDependencyToAssets.ContainsKey(file.FullPath))
                        {
                            // Prepare the hash of the import file in advance for later re-import
                            FileVersionManager.Instance.ComputeFileHashAsync(file.FullPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
                        }
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
                    return;

                var sourceImportFileChangedEventsToAdd = new List<AssetFileChangedEvent>();
                foreach (var itemId in items)
                {
                    AssetItem assetItem;
                    var sourceFiles = mapAssetToInputDependencies[itemId];
                    TrackedAssets.TryGetValue(itemId, out assetItem);
                    if (assetItem == null)
                        continue;

                    bool shouldNotifyChange;
                    if (!sourceFiles.TryGetValue(sourceFile, out shouldNotifyChange))
                        continue;

                    var oldHash = ObjectId.Empty;
                    assetItem.Asset.SourceHashes?.TryGetValue(sourceFile, out oldHash);

                    shouldNotifyChange = oldHash != hash;

                    if (shouldNotifyChange)
                    {
                        // If the hash is empty, the source file has been deleted
                        var changeType = hash == ObjectId.Empty ? AssetFileChangedType.SourceDeleted : AssetFileChangedType.SourceUpdated;

                        // Transmit the hash in the event as well, so that we can check again if the asset has not been updated during the async round-trip
                        // (it happens when re-importing multiple assets at once).
                        sourceImportFileChangedEventsToAdd.Add(new AssetFileChangedEvent(assetItem.Package, changeType, assetItem.Location) { AssetId = assetItem.Id, Hash = hash });
                    }
                }

                if (sourceImportFileChangedEventsToAdd.Count > 0 && !isTrackingPaused)
                {
                    currentAssetFileChangedEvents.AddRange(sourceImportFileChangedEventsToAdd);
                }
                sourceImportFileChangedEventsToAdd.Clear();
            }
        }

        /// <summary>
        /// Visitor that collect all asset references.
        /// </summary>
        private class SourceFilesCollector : AssetVisitorBase
        {
            private Dictionary<UFile, bool> sourceFileMembers;

            public Dictionary<UFile, bool> GetSourceFiles(AssetItem item)
            {
                sourceFileMembers = new Dictionary<UFile, bool>();

                Visit(item.Asset);

                return sourceFileMembers;
            }

            public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
            {
                // Don't visit base parts as they are visited at the top level.
                if (typeof(Asset).IsAssignableFrom(member.DeclaringType) && (member.Name == Asset.BasePartsProperty))
                {
                    return;
                }

                if (member.Type == typeof(UFile) && value != null)
                {
                    var file = (UFile)value;
                    var attribute = member.GetCustomAttributes<SourceFileMemberAttribute>(true).SingleOrDefault();
                    if (attribute != null)
                    {
                        if (!sourceFileMembers.ContainsKey(file))
                        {
                            sourceFileMembers.Add(file, attribute.UpdateAssetIfChanged);
                        }
                        else if (attribute.UpdateAssetIfChanged)
                        {
                            // If the file has already been collected, just update whether it should update the asset when changed
                            sourceFileMembers[file] = true;
                        }
                    }
                }
                base.VisitObjectMember(container, containerDescriptor, member, value);
            }
        }
    }
}
