// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    public struct HitResult
    {
        public PhysicsComponent Collider;

        public Vector3 Normal;

        public Vector3 Point;

        public bool Succeeded;
    }
}