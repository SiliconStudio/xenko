using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tracking
{
    public sealed class AssetSourceTracker : IDisposable
    {
        private readonly PackageSession session;
        internal readonly object ThisLock = new object();
        internal readonly HashSet<Package> Packages;
        private readonly Dictionary<Guid, TrackedAsset> trackedAssets = new Dictionary<Guid, TrackedAsset>();
        // Objects used to track directories
        internal DirectoryWatcher DirectoryWatcher;
        private readonly Dictionary<string, HashSet<Guid>> mapSourceFilesToAssets = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ObjectId> currentHashes = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
        private readonly List<FileEvent> fileEvents = new List<FileEvent>();
        private readonly ManualResetEvent threadWatcherEvent;
        private readonly CancellationTokenSource tokenSourceForImportHash;
        private Thread fileEventThreadHandler;
        private int trackingSleepTime;
        private bool isDisposed;
        private bool isDisposing;
        private bool isTrackingPaused;
        private bool isSaving;

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

            foreach (var package in session.Packages)
            {
                TrackPackage(package);
            }
        }

        /// <summary>
        /// Gets a source dataflow block in which notifications that a source file has changed are pushed.
        /// </summary>
        public BroadcastBlock<IReadOnlyList<SourceFileChangedData>> SourceFileChanged { get; } = new BroadcastBlock<IReadOnlyList<SourceFileChangedData>>(null);

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

        public void BeginSavingSession()
        {
            isSaving = true;
        }

        public void EndSavingSession()
        {
            isSaving = false;
        }

        public ObjectId GetCurrentHash(UFile file)
        {
            ObjectId result;
            currentHashes.TryGetValue(file, out result);
            return result;
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

                    TrackAsset(asset.Id);
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
                    UnTrackAsset(asset.Id);
                }

                Packages.Remove(package);
            }
        }

        /// <summary>
        /// This method is called when an asset needs to be tracked
        /// </summary>
        /// <returns>AssetDependencies.</returns>
        private void TrackAsset(Guid assetId)
        {
            lock (ThisLock)
            {
                if (trackedAssets.ContainsKey(assetId))
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
                var clonedAsset = assetItem.Package.IsSystem ? assetItem.Asset : (Asset)AssetCloner.Clone(assetItem.Asset, AssetClonerFlags.KeepBases);
                var trackedAsset = new TrackedAsset(this, assetItem.Asset, clonedAsset);

                // Adds to global list
                trackedAssets.Add(assetId, trackedAsset);
            }
        }

        private void UnTrackAsset(Guid assetId)
        {
            lock (ThisLock)
            {
                TrackedAsset trackedAsset;
                if (!trackedAssets.TryGetValue(assetId, out trackedAsset))
                    return;

                trackedAsset.Dispose();

                // Remove from global list
                trackedAssets.Remove(assetId);
            }
        }

        internal void TrackAssetImportInput(Guid assetId, string inputPath)
        {
            lock (ThisLock)
            {
                HashSet<Guid> assetsTrackedByPath;
                if (!mapSourceFilesToAssets.TryGetValue(inputPath, out assetsTrackedByPath))
                {
                    assetsTrackedByPath = new HashSet<Guid>();
                    mapSourceFilesToAssets.Add(inputPath, assetsTrackedByPath);
                    DirectoryWatcher?.Track(inputPath);
                }
                assetsTrackedByPath.Add(assetId);
            }

            // We will always issue a compute of the hash in order to verify SourceHash haven't changed
            FileVersionManager.Instance.ComputeFileHashAsync(inputPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
        }

        internal void UnTrackAssetImportInput(Guid assetId, string inputPath)
        {
            lock (ThisLock)
            {
                HashSet<Guid> assetsTrackedByPath;
                if (mapSourceFilesToAssets.TryGetValue(inputPath, out assetsTrackedByPath))
                {
                    assetsTrackedByPath.Remove(assetId);
                    if (assetsTrackedByPath.Count == 0)
                    {
                        mapSourceFilesToAssets.Remove(inputPath);
                        DirectoryWatcher?.UnTrack(inputPath);
                    }
                }
            }
        }

        private void ActivateTracking()
        {
            List<string> files;
            lock (ThisLock)
            {
                files = mapSourceFilesToAssets.Keys.ToList();
            }
            foreach (var inputPath in files)
            {
                DirectoryWatcher.Track(inputPath);
                FileVersionManager.Instance.ComputeFileHashAsync(inputPath, SourceImportFileHashCallback, tokenSourceForImportHash.Token);
            }
        }

        private void Session_AssetDirtyChanged(Asset asset, bool oldValue, bool newValue)
        {
            // Don't update the source tracker while saving
            if (!isSaving)
            {
                lock (ThisLock)
                {
                    TrackedAsset trackedAsset;
                    if (trackedAssets.TryGetValue(asset.Id, out trackedAsset))
                    {
                        trackedAsset.NotifyAssetChanged();
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
                    foreach (AssetItem assetItem in e.NewItems)
                    {
                        TrackAsset(assetItem.Id);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (AssetItem assetItem in e.OldItems)
                    {
                        UnTrackAsset(assetItem.Id);
                    }
                    break;
                default:
                    throw new NotSupportedException("Reset is not supported by the source tracker.");
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

        /// <summary>
        /// This method is running in a separate thread and process file events received from <see cref="Core.IO.DirectoryWatcher"/>
        /// in order to generate the appropriate list of <see cref="AssetFileChangedEvent"/>.
        /// </summary>
        private void RunChangeWatcher()
        {
            while (!threadWatcherEvent.WaitOne(TrackingSleepTime))
            {
                // Use a working copy in order to limit the locking
                var fileEventsWorkingCopy = new List<FileEvent>();

                lock (fileEvents)
                {
                    fileEventsWorkingCopy.AddRange(fileEvents);
                    fileEvents.Clear();
                }

                if (fileEventsWorkingCopy.Count == 0 || isTrackingPaused || isSaving)
                    continue;

                // If this an asset belonging to a package
                lock (ThisLock)
                {
                    // File event
                    foreach (var fileEvent in fileEventsWorkingCopy)
                    {
                        var file = new UFile(fileEvent.FullPath);
                        if (mapSourceFilesToAssets.ContainsKey(file.FullPath))
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
                if (!mapSourceFilesToAssets.TryGetValue(sourceFile, out items))
                    return;

                currentHashes[sourceFile] = hash;

                var message = new List<SourceFileChangedData>();
                foreach (var itemId in items)
                {
                    TrackedAsset trackedAsset;
                    if (trackedAssets.TryGetValue(itemId, out trackedAsset))
                    {
                        bool needUpdate = trackedAsset.DependsOnSource(sourceFile);
                        var data = new SourceFileChangedData(SourceFileChangeType.SourceFile, trackedAsset.AssetId, new[] { sourceFile }, needUpdate);
                        message.Add(data);
                    }
                }
                if (message.Count > 0)
                {
                    SourceFileChanged.Post(message);
                }
            }
        }
    }
}
