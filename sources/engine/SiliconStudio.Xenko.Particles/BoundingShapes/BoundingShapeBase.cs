// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.BoundingShapes
{
    [DataContract("BoundingShapeBase")]
    public abstract class BoundingShapeBase
    {
        [DataMemberIgnore]
        public bool Dirty { get; set; } = true;

        public abstract BoundingBox GetAABB(Vector3 translation, Quaternion rotation, float scale);
    }
}
