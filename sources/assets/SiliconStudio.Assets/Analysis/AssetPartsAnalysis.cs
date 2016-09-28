// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Analysis
{
    public static class AssetPartsAnalysis
    {
        /// <summary>
        /// Remaps the parts identifier.
        /// </summary>
        /// <typeparam name="TAssetPartDesign"></typeparam>
        /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
        /// <param name="hierarchy">The hierarchy of parts.</param>
        /// <param name="idRemapping">The identifier remapping.</param>
        public static void RemapPartsId<TAssetPartDesign, TAssetPart>(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> hierarchy, IDictionary<Guid, Guid> idRemapping)
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            Guid newId;

            // Remap parts in asset2 with new Id
            for (var i = 0; i < hierarchy.RootPartIds.Count; ++i)
            {
                if (idRemapping.TryGetValue(hierarchy.RootPartIds[i], out newId))
                    hierarchy.RootPartIds[i] = newId;
            }

            foreach (var part in hierarchy.Parts)
            {
                if (idRemapping.TryGetValue(part.Part.Id, out newId))
                    part.Part.Id = newId;
            }

            // Sort again the hierarchy (since the Ids changed)
            hierarchy.Parts.Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAssetPartDesign"></typeparam>
        /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
        /// <param name="sourceHierarchy"></param>
        /// <param name="idRemapping">The identifier remapping.</param>
        /// <param name="clonedHierarchy"></param>
        // FIXME: find a better name for this method and fill the summary
        public static void FixPartGroups<TAssetPartDesign, TAssetPart>(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> sourceHierarchy, Dictionary<Guid, Guid> idRemapping, AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> clonedHierarchy)
            where TAssetPartDesign : IAssetPartDesign<TAssetPart>
            where TAssetPart : IIdentifiable
        {
            var baseInstanceMapping = new Dictionary<Guid, Guid>();
            foreach (var ids in idRemapping)
            {
                var source = sourceHierarchy.Parts[ids.Key];
                var clone = clonedHierarchy.Parts[ids.Value];
                if (!source.BasePartInstanceId.HasValue)
                    continue;

                Guid newInstanceId;
                if (!baseInstanceMapping.TryGetValue(source.BasePartInstanceId.Value, out newInstanceId))
                {
                    newInstanceId = Guid.NewGuid();
                    baseInstanceMapping.Add(source.BasePartInstanceId.Value, newInstanceId);
                }
                clone.BasePartInstanceId = newInstanceId;
                clone.BaseId = source.BaseId;
            }
        }
    }
}
