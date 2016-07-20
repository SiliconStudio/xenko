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
        /// <param name="hierarchy">The hierarchy of parts.</param>
        /// <param name="idRemapping">The identifier remapping.</param>
        public static void RemapPartsId<TAssetPartDesign, TAssetPart>(AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> hierarchy, Dictionary<Guid, Guid> idRemapping)
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
    }
}
