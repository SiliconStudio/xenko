// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("PhysicsComponent")]
    [Display(3000, "Physics", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(PhysicsProcessor))]
    public sealed class PhysicsComponent : EntityComponent
    {
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

        [DataMemberIgnore]
        internal PhysicsDebugShapeRendering DebugShapeRendering;
    }
}
