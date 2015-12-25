// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.BoundingShapes
{
    [DataContract("BoundingShape")]
    public abstract class BoundingShape
    {
        [DataMemberIgnore]
        public bool Dirty { get; set; } = true;

        public abstract BoundingBox GetAABB(Vector3 translation, Quaternion rotation, float scale);

        public virtual bool TryGetDebugDrawShape(ref DebugDrawShape debugDrawShape, ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale)
        {
            return false;
        }
    }
}
