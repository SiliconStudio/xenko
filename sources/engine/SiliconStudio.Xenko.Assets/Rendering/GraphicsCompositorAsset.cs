// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract("GraphicsCompositorAsset")]
    [Display(85, "Graphics Compositor")]
    [AssetDescription(FileExtension)]
    [AssetPartReference(typeof(Block))]
    [AssetPartReference(typeof(Link))]
    public class GraphicsCompositorAsset : AssetComposite
    {
        /// <summary>
        /// The default file extension used by the <see cref="AnimationAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";

        public AssetPartCollection<Block> Blocks { get; } = new AssetPartCollection<Block>();

        public AssetPartCollection<Link> Links { get; } = new AssetPartCollection<Link>();

        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var block in Blocks)
                yield return new AssetPart(block.Id, block.BaseId, block.BasePartInstanceId);
            foreach (var link in Links)
                yield return new AssetPart(link.Id, link.BaseId, link.BasePartInstanceId);
        }

        public override bool ContainsPart(Guid id)
        {
            return Blocks.ContainsKey(id) || Links.ContainsKey(id);
        }

        public override void FixupPartReferences()
        {
            AssetCompositeAnalysis.FixupAssetPartReferences(this, ResolveReference);
        }

        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            Block block;
            if (Blocks.TryGetValue(id, out block))
            {
                block.BaseId = baseId;
                block.BasePartInstanceId = basePartInstanceId;
            }

            Link link;
            if (Links.TryGetValue(id, out link))
            {
                link.BaseId = baseId;
                link.BasePartInstanceId = basePartInstanceId;
            }
        }

        protected virtual object ResolveReference(object partReference)
        {
            var blockReference = partReference as Block;
            if (blockReference != null)
            {
                Block realPart;
                Blocks.TryGetValue(blockReference.Id, out realPart);
                return realPart;
            }

            var linkReference = partReference as Link;
            if (linkReference != null)
            {
                Link realPart;
                Links.TryGetValue(linkReference.Id, out realPart);
                return realPart;
            }

            return null;
        }
    }
}