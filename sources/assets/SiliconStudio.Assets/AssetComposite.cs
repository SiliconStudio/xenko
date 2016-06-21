// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract("AssetCompositeHierarchyData")]
    public class AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : IIdentifiable
    {
        [DataMember(10)]
        public List<Guid> RootPartIds { get; } = new List<Guid>();

        [DataMember(20)]
        public AssetPartCollection<TAssetPartDesign, TAssetPart> Parts { get; } = new AssetPartCollection<TAssetPartDesign, TAssetPart>();
    }

    public abstract class AssetCompositeHierarchy<TAssetPartDesign, TAssetPart> : AssetComposite
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        public AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart> Hierarchy { get; set; } = new AssetCompositeHierarchyData<TAssetPartDesign, TAssetPart>();

        public abstract TAssetPart GetParent(TAssetPart part);

        public abstract IEnumerable<TAssetPart> EnumerateChildParts(TAssetPart part, bool isRecursive);

        public IEnumerable<TAssetPartDesign> EnumerateChildParts(TAssetPartDesign partDesign, bool isRecursive)
        {
            return EnumerateChildParts(partDesign.Part, isRecursive).Select(e => Hierarchy.Parts[e.Id]);
        }

        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var part in Hierarchy.Parts)
            {
                yield return new AssetPart(part.Part.Id, part.BaseId, part.BasePartInstanceId);
            }
        }

        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            TAssetPartDesign partEntry;
            if (Hierarchy.Parts.TryGetValue(id, out partEntry))
            {
                partEntry.BaseId = baseId;
                partEntry.BasePartInstanceId = basePartInstanceId;
            }
        }

        public override bool ContainsPart(Guid id)
        {
            return Hierarchy.Parts.ContainsKey(id);
        }

        public override void FixupPartReferences()
        {
            AssetCompositeAnalysis.FixupAssetPartReferences(this, ResolveReference);
        }

        protected virtual object ResolveReference(object partReference)
        {
            var reference = partReference as TAssetPart;
            if (reference != null)
            {
                TAssetPartDesign realPart;
                Hierarchy.Parts.TryGetValue(reference.Id, out realPart);
                return realPart?.Part;
            }
            return null;
        }
    }

    /// <summary>
    /// Base class for an asset that supports inheritance by composition.
    /// </summary>
    public abstract class AssetComposite : Asset, IAssetComposite
    {
        /// <summary>
        /// Adds the given <see cref="AssetBase"/> to the <see cref="Asset.BaseParts"/> collection of this asset.
        /// </summary>
        /// <remarks>If the <see cref="Asset.BaseParts"/> collection already contains the argument. this method does nothing.</remarks>
        /// <param name="newBasePart">The base to add to the <see cref="Asset.BaseParts"/> collection.</param>
        public void AddBasePart(AssetBase newBasePart)
        {
            if (newBasePart == null) throw new ArgumentNullException(nameof(newBasePart));

            if (BaseParts == null)
            {
                BaseParts = new List<AssetBase>();
            }

            if (BaseParts.All(x => x.Id != newBasePart.Id))
            {
                BaseParts.Add(newBasePart);
            }
        }

        public abstract IEnumerable<AssetPart> CollectParts();

        public abstract void SetPart(Guid id, Guid baseId, Guid basePartInstanceId);

        public abstract bool ContainsPart(Guid id);

        public abstract void FixupPartReferences();
    }
}
