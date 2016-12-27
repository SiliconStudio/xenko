// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("ModelNodeLinkComponent")]
    [Display("Model Node Link", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(ModelNodeLinkProcessor))]
    [ComponentOrder(1500)]
    public sealed class ModelNodeLinkComponent : EntityComponent
    {
        private ModelComponent target;

        /// <summary>
        /// Gets or sets the model which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The model which contains the hierarchy to use.
        /// </value>
        /// <userdoc>The reference to the target entity to which attach the current entity. If null, parent will be used.</userdoc>
        [Display("Target (Parent if not set)")]
        public ModelComponent Target
        {
            get
            {
                return target;
            }
            set
            {
                if (value != null && Entity != null && (value.Entity == Entity || !RecurseCheckChildren(Entity.Transform.Children, value.Entity.Transform))) return;
                target = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        /// <userdoc>The name of node of the model of the target entity to which attach the current entity.</userdoc>
        public string NodeName { get; set; }

        private bool RecurseCheckChildren(FastCollection<TransformComponent> children, TransformComponent targetTransform)
        {
            foreach (var transformComponentChild in children)
            {
                if (!RecurseCheckChildren(transformComponentChild.Children, targetTransform))
                    return false;

                if (targetTransform != transformComponentChild)
                    continue;

                return false;
            }
            return true;
        }

        internal void OnHierarchyChanged(object sender, Entity entity)
        {
            if (entity != Target.Entity) return;

            //we want to either throw or clean the target if we target self or a child
            var transformToCheck = entity.Transform;
            var modelTransform = Target?.Entity.Transform;
            if (modelTransform == null) return;

            while (transformToCheck != null)
            {
                if (transformToCheck == modelTransform)
                {
                    Target = null;
                    //todo throw exception or so when we finally have some feedback in the editor
                    return;
                }

                transformToCheck = transformToCheck.Parent;
            }
        }
    }
}