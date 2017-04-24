// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Tests
{
    public interface IAssetSimplePart : IIdentifiable
    {
    }

    public abstract class AssetSimplePart : IAssetSimplePart
    {
        protected AssetSimplePart()
        {
            Id = Guid.NewGuid();
        }

        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }
    }

    public class TopLevelPart : AssetSimplePart
    {
        public AssetSimplePart Part1 { get; set; }

        public AssetSimplePart Part2 { get; set; }
    }

    public class InlinePart : AssetSimplePart
    {
        public AssetSimplePart Part1 { get; set; }

        public AssetSimplePart Part2 { get; set; }
    }

    [AssetPartReference(typeof(IAssetSimplePart))]
    public class AssetWithPartReferences : AssetComposite
    {
        /// <summary>
        /// The list of member variables (properties and fields).
        /// </summary>
        [DataMember(50)]
        public AssetPartCollection<IAssetSimplePart> Parts { get; } = new AssetPartCollection<IAssetSimplePart>();

        /// <inheritdoc/>
        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var part in Parts)
                yield return new AssetPart(part.Id, null, newBase => { });
        }

        /// <inheritdoc/>
        public override IIdentifiable FindPart(Guid partId)
        {
            foreach (var part in Parts)
            {
                if (part.Id == partId)
                    return part;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid partId)
        {
            foreach (var part in Parts)
            {
                if (part.Id == partId)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        protected override object ResolvePartReference(object referencedObject)
        {
            var partReference = referencedObject as AssetSimplePart;
            if (partReference != null)
            {
                foreach (var part in Parts)
                {
                    if (part.Id == partReference.Id)
                        return part;
                }
                return null;
            }

            return null;
        }
    }
}
