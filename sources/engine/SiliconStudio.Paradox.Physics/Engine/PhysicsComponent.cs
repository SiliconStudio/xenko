// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Physics;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("PhysicsComponent")]
    [Display(30, "Physics", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(PhysicsProcessor))]
    public sealed class PhysicsComponent : EntityComponent
    {
        public static PropertyKey<PhysicsComponent> Key = new PropertyKey<PhysicsComponent>("Key", typeof(PhysicsComponent));

        public PhysicsComponent()
        {
            Elements = new List<IPhysicsElement>();
        }

        /// <summary>
        /// Elements describing the physical colliders/rigidbodies/character of this entity
        /// Any runtime dynamic change should happen while the entity is not added to the Game object
        /// </summary>
        [MemberCollection(CanReorderItems = true)]
        public List<IPhysicsElement> Elements { get; private set; }

        [DataMemberIgnore]
        public IPhysicsElement this[int i] => Elements[i];

        [DataMemberIgnore]
        public int Count => Elements.Count;

        [DataMemberIgnore]
        public Simulation Simulation { get; internal set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}
