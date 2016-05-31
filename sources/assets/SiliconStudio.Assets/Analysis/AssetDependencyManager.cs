// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Assets.Tracking;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

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
        private bool isDisposed;
        private bool isInitialized;

        /// <summary>
        /// Occurs when a asset changed. This event is called in the critical section of the dependency manager,
        /// meaning that dependencies can be safely computed via <see cref="ComputeDependencies"/> method from this callback.
        /// </summary>
        public event DirtyFlagChangedDelegate<AssetItem> AssetChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDependencyManager" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException">session</exception>
        internal AssetDependencyManager(PackageSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            this.session = session;
            this.session.Packages.CollectionChanged += Packages_CollectionChanged;
            session.AssetDirtyChanged += Session_AssetDirtyChanged;
            AssetsWithMissingReferences = new Dictionary<Guid, AssetDependencies>();
            MissingReferencesToParent = new Dictionary<Guid, HashSet<AssetDependencies>>();
            Packages = new HashSet<Package>();
            Dependencies = new Dictionary<Guid, AssetDependencies>();
            SourceTracker = new AssetSourceTracker(session);
            // If the session has already a root package, then initialize the dependency manager directly
            if (session.LocalPackages.Any())
            {
                Initialize();
            }
        }

        // TODO: this could be moved directly in PackageSession since it is quite independent - need to find when to initialize it, tho
        public AssetSourceTracker SourceTracker { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized. See remarks.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// If this instance is not initialized, all public methods may block until the full initialization of this instance.
        /// </remarks>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
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
        /// Computes the dependencies for the specified asset.
        /// </summary>
        /// <param name="assetItem">The asset item.</param>
        /// <param name="dependenciesOptions">The dependencies options.</param>
        /// <param name="linkTypes">The type of links to visit while computing the dependencies</param>
        /// <param name="visited">The list of element already visited.</param>
        /// <returns>The dependencies.</returns>
        public AssetDependencies ComputeDependencies(AssetItem assetItem, AssetDependencySearchOptions dependenciesOptions = AssetDependencySearchOptions.All, ContentLinkType linkTypes = ContentLinkType.All, HashSet<Guid> visited = null)
        {
            if (assetItem == null) throw new ArgumentNullException(nameof(assetItem));
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
                    CollectInputReferences(dependencies, assetItem, visited, recursive, linkTypes, ref inCount);
                }

                if ((dependenciesOptions & AssetDependencySearchOptions.Out) != 0)
                {
                    visited?.Clear();
                    CollectOutputReferences(dependencies, assetItem, visited, recursive, linkTypes, ref outCount);
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
        /// <returns>IEnumerable{IReference}.</returns>
        /// <exception cref="System.ArgumentNullException">item</exception>
        public IEnumerable<IReference> FindMissingReferences(Guid assetId)
        {
            lock (Initialize())
            {
                AssetDependencies dependencies;
                if (AssetsWithMissingReferences.TryGetValue(assetId, out dependencies))
                {
                    return dependencies.BrokenLinksOut.Select(x => x.Element).ToList();
                }
            }

            return Enumerable.Empty<IReference>();
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
                if (isDisposed)
                {
                    return ThisLock;
                }

                // Initialize with the list of packages
                foreach (var package in session.Packages)
                {
                    // If the package is cancelled, don't try to do anything
                    // A cancellation means that the package session will be destroyed
                    if (isDisposed)
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
            if (packageSession == null) throw new ArgumentNullException(nameof(packageSession));
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
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (assetResolver == null) throw new ArgumentNullException(nameof(assetResolver));

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
                        result.AddLinkOut(nextItem, link.Type);

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
                    if (isDisposed)
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
                    : new AssetItem(assetItem.Location, (Asset)AssetCloner.Clone(assetItem.Asset, AssetClonerFlags.KeepBases), assetItem.Package)
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
            }

            CheckAllDependencies();
        }

        private void UpdateAssetDependencies(AssetDependencies dependencies)
        {
            lock (ThisLock)
            {
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
                    childDependencyItem?.RemoveLinkIn(dependencies.Item);
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
                    childDependencyItem?.AddLinkIn(dependencies.Item, assetLink.Type);
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
                    parentDependencies.AddLinkOut(item, oldBrokenLink.Type);

                    // Update [In] dependency to current
                    dependencies.AddLinkIn(parentDependencies.Item, oldBrokenLink.Type);

                    // Remove global cache for assets with missing references
                    if (!parentDependencies.HasMissingDependencies)
                    {
                        AssetsWithMissingReferences.Remove(parentDependencies.Item.Id);
                    }
                }
            }
        }

        private void Session_AssetDirtyChanged(Asset asset, bool oldValue, bool newValue)
        {
            // Don't update the dependency manager while saving (setting dirty flag to false)
            if (newValue)
            {
                lock (ThisLock)
                {
                    AssetDependencies dependencies;
                    if (Dependencies.TryGetValue(asset.Id, out dependencies))
                    {
                        dependencies.Item.Asset = (Asset)AssetCloner.Clone(asset, AssetClonerFlags.KeepBases);
                        UpdateAssetDependencies(dependencies);

                        // Notify an asset changed
                        OnAssetChanged(dependencies.Item, oldValue, newValue);
                    }
                }

                CheckAllDependencies();
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

        private void CollectInputReferences(AssetDependencies dependencyRoot, AssetItem assetItem, HashSet<Guid> visited, bool recursive, ContentLinkType linkTypes, ref int count)
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
                    if ((linkTypes & pair.Type) != 0)
                    {
                        dependencyRoot.AddLinkIn(pair);

                        if (visited != null && recursive)
                        {
                            CollectInputReferences(dependencyRoot, pair.Item, visited, true, linkTypes, ref count);
                        }
                    }
                }
            }
        }

        private void CollectOutputReferences(AssetDependencies dependencyRoot, AssetItem assetItem, HashSet<Guid> visited, bool recursive, ContentLinkType linkTypes, ref int count)
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
                if ((linkTypes & child.Type) != 0)
                {
                    dependencyRoot.AddLinkOut(child);

                    if (visited != null && recursive)
                    {
                        CollectOutputReferences(dependencyRoot, child.Item, visited, true, linkTypes, ref count);
                    }
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

        private void OnAssetChanged(AssetItem obj, bool oldValue, bool newValue)
        {
            // Make sure we clone the item here only if it is necessary
            // Cloning the AssetItem is mandatory in order to make sure
            // the asset item won't change
            AssetChanged?.Invoke(obj.Clone(true), oldValue, newValue);
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
                var reference = obj as IReference;
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

                    dependencies.AddBrokenLinkOut(reference, (isBase ? ContentLinkType.Inheritance : 0) | ContentLinkType.Reference);
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
    }
}
