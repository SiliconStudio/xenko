// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Paradox.Physics
{
    [DataConverter(AutoGenerate = false, ContentReference = true)]
    [DataSerializer(typeof(PhysicsColliderShapeSerializer))]
    [ContentSerializer(typeof(DataContentConverterSerializer<PhysicsColliderShape>))]
    public class PhysicsColliderShape
    {
        public PhysicsColliderShape(ColliderShape shape)
        {
            Shape = shape;
        }

        public ColliderShape Shape { get; private set; }
    }
}
