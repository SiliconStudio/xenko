// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.Model.Analysis;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract]
    public class EntityDiffNode : IDataDiffProxy
    {
        private EntityHierarchyData entityHierarchy;
        private TransformationComponentData transformationComponent;

        [DataMember(10)]
        public EntityData Data { get; private set; }

        [DataMember(20)]
        public IList<EntityDiffNode> Children { get; private set; }

        public EntityDiffNode(EntityHierarchyData entityHierarchy, Guid entityGuid)
        {
            this.entityHierarchy = entityHierarchy;

            Data = entityHierarchy.Entities[entityGuid];

            transformationComponent = (TransformationComponentData)Data.Components[TransformationComponent.Key];

            // Build children
            var children = transformationComponent.Children;
            Children = new EntityDiffNode[children.Count];
            for (int i = 0; i < children.Count; ++i)
            {
                Children.Add(new EntityDiffNode(entityHierarchy, children[i].Entity.Id));
            }
        }

        public void ApplyChanges()
        {
            entityHierarchy.Entities.Clear();

            // Rebuild list of children and entityCollection recursively
            ApplyChangesRecursive();

            // Restore root entity Id
            entityHierarchy.RootEntity = Data.Id;

            // Remove references to invalid entities/components
            EntityAnalysis.FixupEntityReferences(entityHierarchy);
        }

        private void ApplyChangesRecursive()
        {
            entityHierarchy.Entities.Add(Data);
            transformationComponent.Children.Clear();
            foreach (var child in Children)
            {
                transformationComponent.Children.Add(new EntityComponentReference<TransformationComponent>(child.transformationComponent));
                child.ApplyChangesRecursive();
            }
        }
    }
}