// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Physics
{
    public interface IPhysicsElement
    {
        Collider Collider { get; }
        RigidBody RigidBody { get; }
        Character Character { get; }
        void UpdatePhysicsTransformation();
    }
}
