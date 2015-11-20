// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    public static class AssetCollision
    {
        /// <summary>
        /// Cleans the specified input items.
        /// </summary>
        /// <param name="package">The package to process (optional).</param>
        /// <param name="inputItems">The input items.</param>
        /// <param name="outputItems">The output items.</param>
        /// <param name="assetResolver">The asset resolver.</param>
        /// <param name="cloneInput">if set to <c>true</c> [clone input].</param>
        /// <exception cref="System.ArgumentNullException">
        /// inputItems
        /// or
        /// outputItems
        /// or
        /// assetResolver
        /// </exception>
        /// <exception cref="System.ArgumentException">List cannot contain null items;inputItems</exception>
        public static void Clean(Package package, ICollection<AssetItem> inputItems, ICollection<AssetItem> outputItems, AssetResolver assetResolver, bool cloneInput)
        {
            if (inputItems == null) throw new ArgumentNullException("inputItems");
            if (outputItems == null) throw new ArgumentNullException("outputItems");
            if (assetResolver == null) throw new ArgumentNullException("assetResolver");

            // Check that all items are non-null
            if (inputItems.Any(item => item == null))
            {
                throw new ArgumentException("List cannot contain null items", "inputItems");
            }

            var items = inputItems;
            if (cloneInput)
            {
                items = inputItems.Select(item => item.Clone()).ToList();
            }

            // Check if locations are conflicting
            var locationConflicts = new Dictionary<AssetItem, UFile>();
            foreach (var item in items)
            {
                UFile newLocation;
                if (assetResolver.RegisterLocation(item.Location, out newLocation))
                {
                    locationConflicts[item] = newLocation;
                }
            }

            // Check if ids are conflicting
            var idConflicts = new Dictionary<AssetItem, Guid>();
            foreach (var item in items)
            {
                Guid newGuid;
                if (assetResolver.RegisterId(item.Id, out newGuid))
                {
                    idConflicts[item] = newGuid;
                }
            }

            // Calculate final guid => guid remapping
            // Because several asset items can have the same id, we are only using the first one for remapping
            var idRemap = new Dictionary<Guid, Tuple<Guid, UFile>>();
            var locationRemap = new Dictionary<UFile, UFile>();

            foreach (var item in items)
            {
                if (outputItems.Contains(item))
                {
                    continue;
                }

                outputItems.Add(item);

                Guid newGuid;
                if (!idConflicts.TryGetValue(item, out newGuid))
                {
                    newGuid = item.Id;
                }

                UFile newLocation;
                if (locationConflicts.TryGetValue(item, out newLocation) && !locationRemap.ContainsKey(item.Location))
                {
                    locationRemap.Add(item.Location, newLocation);
                }

                if (!idRemap.ContainsKey(item.Id))
                {
                    idRemap.Add(item.Id, new Tuple<Guid, UFile>(newGuid, newLocation ?? item.Location));
                }
            }

            // Process assets
            foreach (var item in outputItems)
            {
                // Replace Id
                Guid newGuid;
                if (idConflicts.TryGetValue(item, out newGuid))
                {
                    item.Asset.Id = newGuid;
                    item.IsDirty = true;
                }

                // Replace location
                if (locationConflicts.ContainsKey(item))
                {
                    item.Location = locationConflicts[item];
                    item.IsDirty = true;
                }

                // The loop is a one or two-step. 
                // - If there is no link to update, and the asset has not been cloned, we can exist immediately
                // - If there is links to update, and the asset has not been cloned, we need to clone it and re-enter the loop
                //   to perform the update of the clone asset
                var links = AssetReferenceAnalysis.Visit(item.Asset).Where(link => link.Reference is IContentReference).ToList();

                foreach (var assetLink in links)
                {
                    var assetReference = (IContentReference)assetLink.Reference;

                    var newId = assetReference.Id;
                    var newLocation = assetReference.Location;

                    bool requireUpdate = false;

                    Tuple<Guid, UFile> newRemap;
                    if (idRemap.TryGetValue(newId, out newRemap) && (newId != newRemap.Item1 || newLocation != newRemap.Item2))
                    {
                        newId = newRemap.Item1;
                        newLocation = newRemap.Item2;
                        requireUpdate = true;
                    }

                    UFile remapLocation;
                    if (!requireUpdate && locationRemap.TryGetValue(newLocation, out remapLocation))
                    {
                        newLocation = remapLocation;
                        requireUpdate = true;
                    }

                    if (requireUpdate)
                    {
                        assetLink.UpdateReference(newId, newLocation);
                        item.IsDirty = true;
                    }
                }
            }

            // Process roots (until references in package are handled in general)
            if (package != null)
            {
                UpdateRootAssets(package.RootAssets, idRemap, locationRemap);

                // We check dependencies to be consistent with other places, but nothing should be changed in there
                // (except if we were to instantiate multiple packages referencing each other at once?)
                foreach (var dependency in package.LocalDependencies)
                {
                    if (dependency.RootAssets != null)
                        UpdateRootAssets(dependency.RootAssets, idRemap, locationRemap);
                }
                foreach (var dependency in package.Meta.Dependencies)
                {
                    if (dependency.RootAssets != null)
                        UpdateRootAssets(dependency.RootAssets, idRemap, locationRemap);
                }
            }
        }

        private static void UpdateRootAssets(RootAssetCollection rootAssetCollection, Dictionary<Guid, Tuple<Guid, UFile>> idRemap, Dictionary<UFile, UFile> locationRemap)
        {
            foreach (var rootAsset in rootAssetCollection.ToArray())
            {
                var location = (UFile)rootAsset.Location;
                var id = rootAsset.Id;

                Tuple<Guid, UFile> newId;
                UFile newLocation;

                bool changed = false;
                if (idRemap.TryGetValue(id, out newId))
                {
                    id = newId.Item1;
                    location = newId.Item2;
                    changed = true;
                }
                if (!changed && locationRemap.TryGetValue(location, out newLocation))
                {
                    location = newLocation;
                    changed = true;
                }

                if (changed)
                {
                    var newRootAsset = new AssetReference(id, location);
                    rootAssetCollection.Remove(rootAsset.Id);
                    rootAssetCollection.Add(newRootAsset);
                }
            }
        }
    }
}