// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Physics;

namespace SiliconStudio.Paradox.Physics
{
    public interface IColliderShapeDesc
    {
    }

    [DataContract("PhysicsColliderShapeData")]
    [ContentSerializer(typeof(DataContentSerializer<PhysicsColliderShapeData>))]
    [ContentSerializer(typeof(DataContentConverterSerializer<PhysicsColliderShape>))]
    public class PhysicsColliderShapeData
    {
        public PhysicsColliderShapeData()
        {
            ColliderShapes = new List<IColliderShapeDesc>();
        }

        /// <userdoc>
        /// The collection of shapes in this asset, a collection shapes will automatically generate a compound shape.
        /// </userdoc>
        [DataMember(10)]
        public List<IColliderShapeDesc> ColliderShapes { get; set; }
    }
}
