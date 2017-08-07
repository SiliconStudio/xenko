// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ColliderShapeAssetDesc>))]
    [DataContract("ColliderShapeAssetDesc")]
    [Display(50, "Asset")]
    public class ColliderShapeAssetDesc : IInlineColliderShapeDesc
    {
        /// <userdoc>
        /// The reference to the collider Shape asset.
        /// </userdoc>
        [DataMember(10)]
        public PhysicsColliderShape Shape { get; set; }

        public bool Match(object obj)
        {
            var other = obj as ColliderShapeAssetDesc;
            if (other == null) return false;

            if (other.Shape == null || Shape == null)
                return other.Shape == Shape;

            if (other.Shape.Descriptions == null || Shape.Descriptions == null)
                return other.Shape.Descriptions == Shape.Descriptions;

            if (other.Shape.Descriptions.Count != Shape.Descriptions.Count)
                return false;

            if (other.Shape.Descriptions.Where((t, i) => !t.Match(Shape.Descriptions[i])).Any())
                return false;

            // TODO: shouldn't we return true here?
            return other.Shape == Shape;
        }
    }
}
