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
            List<AssetBase> existingBaseParts = null;

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
                existingBaseParts = new List<AssetBase>();

                foreach (var basePart in assetItem.Asset.BaseParts)
                {
                    AssetItem existingAssetBasePart;
                    if (!ProcessMergeAssetBase(basePart, beingProcessed, out existingAssetBasePart))
                    {
                        return false;
                    }

                    // Replicate the group with the list of ids
                    var newBasePart =new AssetBase(existingAssetBasePart.Location, (Asset)AssetCloner.Clone(existingAssetBasePart.Asset));
                     
                    existingBaseParts.Add(newBasePart);
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

        private bool MergeAsset(AssetItem item, AssetItem existingBase, List<AssetBase> existingBaseParts)
        {
            // No need to clone existingBaseParts as they are already cloned
            var baseCopy = (Asset)AssetCloner.Clone(item.Asset.Base?.Asset);
            var newBaseCopy = (Asset)AssetCloner.Clone(existingBase?.Asset);

            // Computes the hash on the clone (so that we don't have a .Base/.BaseParts in them
            // TODO: We might want to store the hash in the asset in order to avoid a recompute at load time
            // (but would require a compute at save time)
            var baseId = AssetHash.Compute(baseCopy);
            var newBaseCopyId = AssetHash.Compute(newBaseCopy);

            // If the old base and new base are similar (including ids and overrides), we don't need to perform a merge
            if (baseId == newBaseCopyId)
            {
                return true;
            }

            // Delegates actual merge to the asset implem
            var result = item.Asset.Merge(baseCopy, newBaseCopy, existingBaseParts);

            if (result.HasErrors)
            {
                result.CopyTo(log);
                return false;
            }

            item.Asset = (Asset)result.Asset;
            if (item.Asset.Base != null)
            {
                item.Asset.Base = newBaseCopy != null ? new AssetBase(item.Asset.Base.Location, newBaseCopy) : null;
            }

            // Use new existing base parts
            if (existingBaseParts != null)
            {
                item.Asset.BaseParts = existingBaseParts;
            }

            item.IsDirty = true;
            return true;
        }
    }
}