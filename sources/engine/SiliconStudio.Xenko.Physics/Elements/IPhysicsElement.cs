// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Physics
{
    public interface IPhysicsElement
    {
        Collider Collider { get; }

        void UpdatePhysicsTransformation();
    }
}
