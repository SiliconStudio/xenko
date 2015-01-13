// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Physics
{
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
