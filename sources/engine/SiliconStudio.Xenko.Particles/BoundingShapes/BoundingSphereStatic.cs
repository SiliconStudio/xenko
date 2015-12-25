// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.BoundingShapes
{
    [DataContract("BoundingSpheretatic")]
    public class BoundingSphereStatic : BoundingShapeBase
    {
        /// <summary>
        /// Fixed radius of the <see cref="BoundingSphereStatic"/>
        /// </summary>
        /// <userdoc>
        /// Fixed radius of the bounding sphere. Gets calculated as a AABB, which is a cube with corners (-R, -R, -R) - (+R, +R, +R)
        /// </userdoc>
        [DataMember(20)]
        public float Radius { get; set; } = 1f;

        [DataMemberIgnore]
        private BoundingBox cachedBox;
        
        public override BoundingBox GetAABB(Vector3 translation, Quaternion rotation, float scale)
        {
            if (Dirty)
            {
                var r = Radius*scale;

                cachedBox = new BoundingBox(new Vector3(-r, -r, -r) + translation, new Vector3(r, r, r) + translation);
            }

            return cachedBox;
        }

    }
}
