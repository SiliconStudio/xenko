// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Analysis
{
    public static class AssetPartsAnalysis
    {
        /// <summary>
        /// Assigns new unique identifiers for base part <see cref="BasePart.InstanceId"/> in the given <paramref name="hierarchy"/>.
        /// </summary>
        /// <typeparam name="TAssetPartDesign"></typeparam>
        /// <typeparam name="TAssetPart">The underlying type of part.</typeparam>
        /// <param name="hierarchy">The hierarchy which part groups should have new identifiers.</param>
        public static void GenerateNewBaseInstanceIds<TAssetPartDesign, TAssetPart>([NotNull] AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> hierarchy)
            where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
            where TAssetPart : class, IIdentifiable
        {
            var baseInstanceMapping = new Dictionary<Guid, Guid>();
            foreach (var part in hierarchy.Parts)
            {
                if (part.Base == null)
                    continue;

                Guid newInstanceId;
                if (!baseInstanceMapping.TryGetValue(part.Base.InstanceId, out newInstanceId))
                {
                    newInstanceId = Guid.NewGuid();
                    baseInstanceMapping.Add(part.Base.InstanceId, newInstanceId);
                }
                part.Base = new BasePart(part.Base.BasePartAsset, part.Base.BasePartId, newInstanceId);
            }
        }
    }
}
