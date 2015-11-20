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
    internal class PackageAssetTemplatingAnalysis
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
                if (assetItem.Asset.Base == null && (assetItem.Asset.BaseParts == null || assetItem.Asset.BaseParts.Count == 0))
                {
                    assetsProcessed.Add(assetItem.Id, assetItem);
                    continue;
                }

                assetsToProcess.Add(assetItem.Id, assetItem);
            }

            // Process assets
            var beingProcessed = new HashSet<Guid>();
            while (assetsToProcess.Count > 0)
            {
                var countBefore = assetsToProcess.Count;

                // Process templating for assets
                foreach (var assetIt in assetsToProcess.Values.ToList())
                {
                    beingProcessed.Clear();
                    ProcessMergeAssetItemFromBase(assetIt, beingProcessed);
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

        private bool ProcessMergeAssetItemFromBase(AssetItem assetItem, HashSet<Guid> beingProcessed)
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
                if (!ProcessAssetBase(assetItem.Asset.Base, beingProcessed, out existingAssetBase))
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
                    if (!ProcessAssetBase(assetBase, beingProcessed, out existingAssetBasePart))
                    {
                        return false;
                    }
                    existingBaseParts.Add(existingAssetBasePart);
                }
            }

            // For simple merge (base, newAsset, newBase) => newObject
            if (existingAssetBase != null && existingBaseParts == null)
            {
                MergeAsset(assetItem, existingAssetBase);
            }
            else
            {
                // For multi-part prefabs merge (base, newAsset, newBase) + baseParts + newBaseParts => newObject
            }

            assetsProcessed.Add(assetItem.Id, assetItem);
            assetsToProcess.Remove(assetItem.Id);


            return true;
        }

        private void MergeAsset(AssetItem item, AssetItem newBase)
        {
            var result = AssetMerge.Merge(item.Asset.Base.Asset, AssetCloner.Clone(item.Asset), newBase, MergePolicy);
            
            // TODO: Handle errors...etc.

            item.Asset = (Asset)result.Asset;
            item.IsDirty = true;
        }

        private Diff3ChangeType MergePolicy(Diff3Node node)
        {
            // TODO: Handle merge with overrides...etc.

            return AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1(node);
        }

        private bool ProcessAssetBase(AssetBase assetBase, HashSet<Guid> beingProcessed, out AssetItem existingAsset)
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
                    if (!ProcessMergeAssetItemFromBase(existingAsset, beingProcessed))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}