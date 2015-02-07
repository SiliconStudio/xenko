// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    public class ModelNodeLinkProcessor : EntityProcessor<ModelNodeLinkComponent>
    {
        internal HashSet<ModelNodeLinkComponent> DirtyLinks = new HashSet<ModelNodeLinkComponent>();
        internal ModelProcessor meshProcessor;

        public ModelNodeLinkProcessor()
            : base(new PropertyKey[] { TransformationComponent.Key, ModelNodeLinkComponent.Key })
        {
        }

        protected override ModelNodeLinkComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(ModelNodeLinkComponent.Key);
        }

        protected override void OnEntityAdding(Entity entity, ModelNodeLinkComponent modelNodeLinkComponent)
        {
            entity.Transform.UseTRS = false;
            entity.Transform.isSpecialRoot = true;

            modelNodeLinkComponent.Processor = this;

            if (meshProcessor == null)
                meshProcessor = EntitySystem.GetProcessor<ModelProcessor>();

            lock (DirtyLinks)
            {
                DirtyLinks.Add(modelNodeLinkComponent);

                // Mark it as invalid
                modelNodeLinkComponent.EntityLink.NodeIndex = -1;
            }
        }

        protected override void OnEntityRemoved(Entity entity, ModelNodeLinkComponent modelNodeLinkComponent)
        {
            if (meshProcessor == null)
                meshProcessor = EntitySystem.GetProcessor<ModelProcessor>();

            meshProcessor.UnlinkEntity(modelNodeLinkComponent.EntityLink);

            modelNodeLinkComponent.Processor = null;
        }

        public override void Draw(GameTime time)
        {
            lock (DirtyLinks)
            {
                if (DirtyLinks.Count == 0)
                    return;

                if (meshProcessor == null)
                    meshProcessor = EntitySystem.GetProcessor<ModelProcessor>();

                foreach (var transformationLinkComponent in DirtyLinks)
                {
                    // ModelNodeLinkComponent has been changed, regenerate link
                    meshProcessor.UnlinkEntity(transformationLinkComponent.EntityLink);
                    meshProcessor.LinkEntity(transformationLinkComponent.Entity, transformationLinkComponent.Target, transformationLinkComponent.NodeName);
                }

                DirtyLinks.Clear();
            }
        }
    }
}