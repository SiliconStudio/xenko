// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets.Entities
{
    [DataContract("EntityDiffNode")]
    public class EntityDiffNode : IDiffKey
    {
        [DataMember(10)]
        public string Name { get; set; }

        [DataMember(20)]
        public Guid Guid { get; set; }

        [DataMember(30)]
        public List<EntityDiffNode> Children { get; private set; }

        public EntityDiffNode()
        {
            Children = new List<EntityDiffNode>();
        }

        public EntityDiffNode(EntityHierarchyData entityHierarchy)
            : this(entityHierarchy, entityHierarchy.RootEntity)
        {
        }

        public EntityDiffNode(EntityHierarchyData entityHierarchy, Guid entityGuid)
        {
            var entityData = entityHierarchy.Entities[entityGuid];

            Guid = entityGuid;
            Name = entityData.Name;

            var transformationComponent = entityData.Get(TransformComponent.Key);
            if (transformationComponent != null)
            {
                Children = new List<EntityDiffNode>(transformationComponent.Children.Count);

                // Build children
                var children = transformationComponent.Children;
                for (int i = 0; i < children.Count; ++i)
                {
                    Children.Add(new EntityDiffNode(entityHierarchy, children[i].Entity.Id));
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2} children)", Guid, Name, Children != null ? Children.Count : 0);
        }

        object IDiffKey.GetDiffKey()
        {
            return Name;
        }
    }
}