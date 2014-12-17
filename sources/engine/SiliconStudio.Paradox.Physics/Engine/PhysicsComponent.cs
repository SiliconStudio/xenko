// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Physics
{
    [DataConverter(AutoGenerate = true)]
    [DataContract("PhysicsComponent")]
    public sealed class PhysicsComponent : EntityComponent
    {
        public static PropertyKey<PhysicsComponent> Key = new PropertyKey<PhysicsComponent>("Key", typeof(PhysicsComponent));

        public PhysicsComponent()
        {
            Elements = new List<PhysicsElement>();
        }

        /// <summary>
        /// Elements describing the physical colliders/rigidbodies/character of this entity
        /// Any runtime dynamic change should happen while the entity is not added to the Game object
        /// </summary>
        [DataMemberConvert]
        public List<PhysicsElement> Elements { get; private set; }

        [DataMemberIgnore]
        public PhysicsElement this[int i]
        {
            get { return Elements[i]; }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}
