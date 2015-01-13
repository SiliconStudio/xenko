// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Physics
{
    [DataSerializer(typeof(PhysicsColliderShapeSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<PhysicsColliderShape>))]
    [DataSerializerGlobal(typeof(CloneSerializer<PhysicsColliderShape>), Profile = "Clone")] 
    public class PhysicsColliderShape
    {
        public PhysicsColliderShape(ColliderShape shape)
        {
            Shape = shape;
        }

        public ColliderShape Shape { get; private set; }
    }
}
