// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Allows to perform asset templating at load time when a base asset could change in one branch, while another branch as already derived a new asset from the base asset.
    /// </summary>
    public class PackageAssetTemplatingAnalysis
    {
        // TODO: The current code doesn't perform skip optimization and always tries to merge assets base/derived, even things didn't change from base.
        // In order to support this skip optimization, we will have to add a content hash when loading assets, this hash will be used to check if content has changed.
        // The hash should include shadow informations (like overrides)

        private readonly Package package;
        private readonly Dictionary<Guid, AssetItem> assetsToProcess;
        private readonly Dictionary<Guid, AssetItem> assetsProcessed;
        private readonly PackageSession session;
        private readonly ILogger log;

        public PackageAssetTemplatingAnalysis(Package package, ILogger log)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (log == null) throw new ArgumentNullException(nameof(log));
            assetsToProcess = new Dictionary<Guid, AssetItem>();
            assetsProcessed = new Dictionary<Guid, AssetItem>();

            this.package = package;
            session = package.Session;
            this.log = log;
        }

        public void Run()
        {
            foreach (var assetItem in package.Assets)
            {
                // If an asset doesn't have any base for templating, we can skip this part
                if ((assetItem.Asset.Base == null || assetItem.Asset.Base.IsRootImport) && (assetItem.Asset.BaseParts == null || assetItem.Asset.BaseParts.Count == 0))
                {
                    assetsProcessed.Add(assetItem.Id, assetItem);
                    continue;
                }

                assetsToProcess.Add(assetItem.Id, assetItem);
            }

            // Because inheritance can happen into the current package, we need to process base assets first before trying to process child asset
            var beingProcessed = new HashSet<Guid>();
            while (assetsToProcess.Count > 0)
            {
                var countBefore = assetsToProcess.Count;

                // Process templating for assets
                foreach (var assetIt in assetsToProcess.Values.ToList())
                {
                    // Use beingProcessed list to verify that we don't have circular references
                    beingProcessed.Clear();
                    ProcessMergeAssetItem(assetIt, beingProcessed);
                }

                // We have to make sure that we have been processing at least one asset during the previous loop
                // otherwise it means that we got an error (recursive assets, base not found...etc.)
                // TODO: in that case we would like still to handle templating for remaining assets
                if (countBefore == assetsToProcess.Count)
                {
                    log.Error("Unexpected error while processing asset templating");
                    break;
                }
            }
        }

        private bool ProcessMergeAssetItem(AssetItem assetItem, HashSet<Guid> beingProcessed)
        {
            if (beingProcessed.Contains(assetItem.Id))
            {
                log.Error(package, assetItem.Asset.Base, AssetMessageCode.AssetNotFound, assetItem.Asset.Base);
                return false;
            }
            beingProcessed.Add(assetItem.Id);

            AssetItem existingAssetBase = null;
            List<AssetItem> existingBaseParts = null;

            // Process asset base
            if (assetItem.Asset.Base != null)
            {
                if (!ProcessMergeAssetBase(assetItem.Asset.Base, beingProcessed, out existingAssetBase))
                {
                    return false;
                }
            }

            // Process asset base parts
            if (assetItem.Asset.BaseParts != null && assetItem.Asset.BaseParts.Count > 0)
            {
                existingBaseParts = new List<AssetItem>(assetItem.Asset.BaseParts.Count);

                foreach (var assetBase in assetItem.Asset.BaseParts)
                {
                    AssetItem existingAssetBasePart;
                    if (!ProcessMergeAssetBase(assetBase, beingProcessed, out existingAssetBasePart))
                    {
                        return false;
                    }
                    existingBaseParts.Add(existingAssetBasePart);
                }
            }

            // For simple merge (base, newAsset, newBase) => newObject
            // For multi-part prefabs merge (base, newAsset, newBase) + baseParts + newBaseParts => newObject
            if (!MergeAsset(assetItem, existingAssetBase, existingBaseParts))
            {
                return false;
            }

            assetsProcessed.Add(assetItem.Id, assetItem);
            assetsToProcess.Remove(assetItem.Id);

            return true;
        }

        private bool ProcessMergeAssetBase(AssetBase assetBase, HashSet<Guid> beingProcessed, out AssetItem existingAsset)
        {
            var baseId = assetBase.Id;

            // Make sure that the base asset exist
            existingAsset = session.FindAsset(baseId);
            if (existingAsset == null)
            {
                log.Warning(package, assetBase, AssetMessageCode.AssetNotFound, assetBase);
                return false;
            }

            // If the base asset hasn't been processed, continue on next asset
            if (!assetsProcessed.ContainsKey(baseId))
            {
                // If asset is in the same package, we can process it right away
                if (existingAsset.Package == package)
                {
                    if (!ProcessMergeAssetItem(existingAsset, beingProcessed))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool MergeAsset(AssetItem item, AssetItem existingBase, List<AssetItem> existingBaseParts)
        {
            var baseCopy = (Asset)AssetCloner.Clone(item.Asset.Base?.Asset);
            var newBase = (Asset)AssetCloner.Clone(existingBase?.Asset);
            var merger = item.Asset ?? newBase ?? baseCopy;

            // Delegates actual merge to the asset implem
            var result = merger.Merge(baseCopy, item.Asset, newBase, existingBaseParts);

            if (result.HasErrors)
            {
                result.CopyTo(log);
                return false;
            }

            item.Asset = (Asset)result.Asset;
            if (item.Asset.Base != null)
            {
                item.Asset.Base = newBase != null ? new AssetBase(item.Asset.Base.Location, newBase) : null;
            }
            item.IsDirty = true;
            return true;
        }
    }
}