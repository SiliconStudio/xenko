// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SiliconStudio.Assets.Diagnostics;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

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
            MergeModifiedAssets = true;
            CheckAndFixInvalidBasePartInstances = true;
            RemoveUnusedBaseParts = true;
        }

        public bool MergeModifiedAssets { get; set; }

        public bool RemoveUnusedBaseParts { get; set; }


        public bool CheckAndFixInvalidBasePartInstances { get; set; }


        public void Run()
        {
            if (RemoveUnusedBaseParts)
            {
                ProcessRemoveUnusedBaseParts();
            }

            if (MergeModifiedAssets)
            {
                ProcessMergeModifiedAssets();
            }

            if (CheckAndFixInvalidBasePartInstances)
            {
                ProcessCheckAndFixInvalidBasePartInstances();
            }
        }

        private void ProcessMergeModifiedAssets()
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

        /// <summary>
        /// This method is responsible to remove AssetBase in BaseParts that are no longer used by any assets.
        /// </summary>
        private void ProcessRemoveUnusedBaseParts()
        {
            var basePartsToKeep = new HashSet<AssetBase>();
            var partInstanceIdProcessed = new HashSet<Guid>();
            foreach (var assetItem in package.Assets)
            {
                var asset = assetItem.Asset as AssetComposite;
                // If an asset doesn't have any base for templating, we can skip this part
                if (asset?.BaseParts == null)
                {
                    continue;
                }

                basePartsToKeep.Clear();
                partInstanceIdProcessed.Clear();

                foreach (var part in asset.CollectParts())
                {
                    if (part.BaseId.HasValue && part.BasePartInstanceId.HasValue && !partInstanceIdProcessed.Contains(part.BasePartInstanceId.Value))
                    {
                        // Add to this map to avoid processing assets from the same BasePartInstanceId
                        partInstanceIdProcessed.Add(part.BasePartInstanceId.Value);

                        var baseId = part.BaseId.Value;
                        foreach (var basePart in asset.BaseParts)
                        {
                            var assetBase = (AssetComposite)basePart.Asset;
                            if (assetBase.ContainsPart(baseId))
                            {
                                basePartsToKeep.Add(basePart);
                            }
                        }
                    }
                }

                for (int i = asset.BaseParts.Count - 1; i >= 0; i--)
                {
                    var basePart = asset.BaseParts[i];
                    if (!basePartsToKeep.Contains(basePart))
                    {
                        asset.BaseParts.RemoveAt(i);
                    }
                }

                if (asset.BaseParts.Count == 0)
                {
                    asset.BaseParts = null;
                }
            }
        }

        /// <summary>
        /// This method is responsible to fix duplicated/invalid asset parts that are referencing the same baseId/basePartInstanceId while it should be unique.
        /// (Typically, we had a problem in the GameStudio that was generating a copy of the same asset with the same baseId/basePartInstanceId, but with
        /// different instances, while we expect to have a different basePartInstanceId for each new instances)
        /// </summary>
        private void ProcessCheckAndFixInvalidBasePartInstances()
        {
            var instances = new HashSet<BasePartInstanceKey>();
            foreach (var assetItem in package.Assets)
            {
                var asset = assetItem.Asset as AssetComposite;
                // If an asset doesn't have any base for templating, we can skip this part
                if (asset?.BaseParts == null)
                {
                    continue;
                }

                foreach (var part in asset.CollectParts())
                {
                    if (part.BaseId.HasValue && part.BasePartInstanceId.HasValue)
                    {
                        // If a part (eg: entity) with the same BaseId+BasePartInstanceId is found, It means that we had an invalid duplicate of this
                        // So we will create a new GUID for BasePartInstanceId and associate the part with it.
                        var key = new BasePartInstanceKey(part.BaseId.Value, part.BasePartInstanceId.Value);
                        if (!instances.Add(key))
                        {
                            // Log a warning
                            log.Warning(package, assetItem.ToReference(), AssetMessageCode.InvalidBasePartInstance, part.Id, $"BaseId:{key.BaseId}, InstanceId: {key.BasePartInstanceId}");

                            // Because it would be too complex (or not possible) to re-associate a proper BasePartInstanceId
                            // We are creating a new BasePartInstanceId for each new duplicated
                            asset.SetPart(part.Id, key.BaseId, Guid.NewGuid());
                            if (!assetItem.IsDirty)
                            {
                                assetItem.IsDirty = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is responsible to merge assets when loading. A 3-way merge of the asset is necessary because concurrent changes
        /// can happen on assets (people working with a SCM and different branches). This method will collect all assets that are 
        /// using asset templating (including prefabs) and will perform the merge iteratively (incremental merge from the most inherited
        /// assets to the most derived asset)
        /// </summary>
        /// <param name="assetItem"></param>
        /// <param name="beingProcessed"></param>
        /// <returns></returns>
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

            // Don't process an asset that has been already processed
            if (!assetsProcessed.ContainsKey(assetItem.Id))
            {
                // For simple merge (base, newAsset, newBase) => newObject
                // For multi-part prefabs merge (base, newAsset, newBase) + baseParts + newBaseParts => newObject
                if (!MergeAsset(assetItem, existingAssetBase, existingBaseParts))
                {
                    return false;
                }

                assetsProcessed.Add(assetItem.Id, assetItem);
                assetsToProcess.Remove(assetItem.Id);
            }

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

        private static bool CompareAssets(Asset left, Asset right)
        {
            // Computes the hash on the clone (so that we don't have a .Base/.BaseParts in them
            // TODO: We might want to store the hash in the asset in order to avoid a recompute at load time
            // (but would require a compute at save time)
            var baseId = AssetHash.Compute(left);
            var newBaseCopyId = AssetHash.Compute(right);

            // If the old base and new base are similar (including ids and overrides), we don't need to perform a merge
            return baseId == newBaseCopyId;
        }

        private bool MergeAsset(AssetItem item, AssetItem existingBase, List<AssetBase> existingBaseParts)
        {
            // No need to clone existingBaseParts as they are already cloned
            var baseCopy = (Asset)AssetCloner.Clone(item.Asset.Base?.Asset);
            var newBaseCopy = (Asset)AssetCloner.Clone(existingBase?.Asset);

            // Check base parts
            bool basePartsAreEqual = true;
            if (item.Asset != null && item.Asset.BaseParts != null)
            {
                foreach (var assetBasePart in item.Asset.BaseParts)
                {
                    var existingBasePart = existingBaseParts.First(e => e.Id == assetBasePart.Asset.Id);
                    if (!CompareAssets(assetBasePart.Asset, existingBasePart.Asset))
                    {
                        basePartsAreEqual = false;
                    }
                }
            }

            // If the old base and new base are similar (including ids and overrides), we don't need to perform a merge
            if (CompareAssets(baseCopy, newBaseCopy) && basePartsAreEqual)
            {
                return true;
            }

            // Delegates actual merge to the asset implem
            var result = item.Asset.Merge(baseCopy, newBaseCopy, existingBaseParts, item.Location);

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

            // Set this variable to true at debug time to check what is the output of the merge
            bool writeToDebug = false;
            if (writeToDebug)
            {
                var writer = new MemoryStream();
                YamlSerializer.Serialize(writer, item.Asset);
                writer.Flush();
                writer.Position = 0;
                var text = Encoding.UTF8.GetString(writer.ToArray());
                Debug.WriteLine(text);
            }

            item.IsDirty = true;
            return true;
        }

        private struct BasePartInstanceKey : IEquatable<BasePartInstanceKey>
        {
            public BasePartInstanceKey(Guid baseId, Guid basePartInstanceId)
            {
                BaseId = baseId;
                BasePartInstanceId = basePartInstanceId;
            }

            public readonly Guid BaseId;

            public readonly Guid BasePartInstanceId;

            public bool Equals(BasePartInstanceKey other)
            {
                return BaseId.Equals(other.BaseId) && BasePartInstanceId.Equals(other.BasePartInstanceId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is BasePartInstanceKey && Equals((BasePartInstanceKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (BaseId.GetHashCode() * 397) ^ BasePartInstanceId.GetHashCode();
                }
            }

            public static bool operator ==(BasePartInstanceKey left, BasePartInstanceKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(BasePartInstanceKey left, BasePartInstanceKey right)
            {
                return !left.Equals(right);
            }
        }
    }
}