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
            if (inputItems == null) throw new ArgumentNullException(nameof(inputItems));
            if (outputItems == null) throw new ArgumentNullException(nameof(outputItems));
            if (assetResolver == null) throw new ArgumentNullException(nameof(assetResolver));

            // Check that all items are non-null
            if (inputItems.Any(item => item == null))
            {
                throw new ArgumentException("List cannot contain null items", nameof(inputItems));
            }

            var items = inputItems;
            if (cloneInput)
            {
                items = inputItems.Select(item => item.Clone()).ToList();
            }

            // idRemap should contain only assets that have either 1) their id remapped or 2) their location remapped
            var idRemap = new Dictionary<Guid, Tuple<Guid, UFile>>();
            var itemRemap = new Dictionary<AssetItem, Tuple<Guid, UFile>>();
            foreach (var item in items)
            {
                if (outputItems.Contains(item))
                {
                    continue;
                }

                outputItems.Add(item);

                bool changed = false;
                Guid newGuid;
                if (assetResolver.RegisterId(item.Id, out newGuid))
                {
                    changed = true;
                }

                UFile newLocation;
                if (assetResolver.RegisterLocation(item.Location, out newLocation))
                {
                    changed = true;
                }

                var tuple = new Tuple<Guid, UFile>(newGuid != Guid.Empty ? newGuid : item.Id, newLocation ?? item.Location);
                if (changed)
                {
                    if (!itemRemap.ContainsKey(item))
                    {
                        itemRemap.Add(item, tuple);
                    }
                }

                if (!idRemap.ContainsKey(item.Id))
                {
                    idRemap.Add(item.Id, tuple);
                }
            }

            // Process assets
            foreach (var item in outputItems)
            {
                Tuple<Guid, UFile> remap;
                if (itemRemap.TryGetValue(item, out remap) && (remap.Item1 != item.Asset.Id || remap.Item2 != item.Location))
                {
                    item.Asset.Id = remap.Item1;
                    item.Location = remap.Item2;
                    item.IsDirty = true;
                }

                // The loop is a one or two-step. 
                // - If there is no link to update, and the asset has not been cloned, we can exist immediately
                // - If there is links to update, and the asset has not been cloned, we need to clone it and re-enter the loop
                //   to perform the update of the clone asset
                var links = AssetReferenceAnalysis.Visit(item.Asset).Where(link => link.Reference is IReference).ToList();

                foreach (var assetLink in links)
                {
                    var assetReference = (IReference)assetLink.Reference;

                    var newId = assetReference.Id;
                    if (idRemap.TryGetValue(newId, out remap) && IsNewReference(remap, assetReference))
                    {
                        assetLink.UpdateReference(remap.Item1, remap.Item2);
                        item.IsDirty = true;
                    }
                }

                // Fix base if there are any
                if (item.Asset.Base != null && idRemap.TryGetValue(item.Asset.Base.Id, out remap) && IsNewReference(remap, item.Asset.Base))
                {
                    item.Asset.Base.Asset.Id = remap.Item1;
                    item.Asset.Base = new AssetBase(remap.Item2, item.Asset.Base.Asset);
                    item.IsDirty = true;
                }

                // Fix base parts if there are any remap for them as well
                if (item.Asset.BaseParts != null)
                {
                    for (int i = 0; i < item.Asset.BaseParts.Count; i++)
                    {
                        var basePart = item.Asset.BaseParts[i];
                        if (idRemap.TryGetValue(basePart.Id, out remap) && IsNewReference(remap, basePart))
                        {
                            basePart.Asset.Id = remap.Item1;
                            item.Asset.BaseParts[i] = new AssetBase(remap.Item2, basePart.Asset);
                            item.IsDirty = true;
                        }
                    }
                }
            }

            // Process roots (until references in package are handled in general)
            if (package != null)
            {
                UpdateRootAssets(package.RootAssets, idRemap);

                // We check dependencies to be consistent with other places, but nothing should be changed in there
                // (except if we were to instantiate multiple packages referencing each other at once?)
                foreach (var dependency in package.LocalDependencies)
                {
                    if (dependency.RootAssets != null)
                        UpdateRootAssets(dependency.RootAssets, idRemap);
                }
                foreach (var dependency in package.Meta.Dependencies)
                {
                    if (dependency.RootAssets != null)
                        UpdateRootAssets(dependency.RootAssets, idRemap);
                }
            }
        }

        private static void UpdateRootAssets(RootAssetCollection rootAssetCollection, Dictionary<Guid, Tuple<Guid, UFile>> idRemap)
        {
            foreach (var rootAsset in rootAssetCollection.ToArray())
            {
                var id = rootAsset.Id;
                Tuple<Guid, UFile> remap;

                if (idRemap.TryGetValue(id, out remap) && IsNewReference(remap, rootAsset))
                {
                    var newRootAsset = new AssetReference<Asset>(remap.Item1, remap.Item2);
                    rootAssetCollection.Remove(rootAsset.Id);
                    rootAssetCollection.Add(newRootAsset);
                }
            }
        }

        private static bool IsNewReference(Tuple<Guid, UFile> newReference, IReference previousReference)
        {
            return newReference.Item1 != previousReference.Id ||
                   newReference.Item2 != previousReference.Location;
        }
    }
}
