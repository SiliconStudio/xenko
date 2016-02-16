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

        // ReSharper disable once InconsistentNaming
        public abstract BoundingBox GetAABB(Vector3 translation, Quaternion rotation, float scale);

        public virtual bool TryGetDebugDrawShape(out DebugDrawShape debugDrawShape, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            debugDrawShape = DebugDrawShape.None;
            scale = new Vector3(1, 1, 1);
            translation = new Vector3(0, 0, 0);
            rotation = new Quaternion(0, 0, 0, 1);
            return false;
        }
    }
}
