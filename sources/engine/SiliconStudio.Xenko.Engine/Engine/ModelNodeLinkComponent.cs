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
        private ModelComponent internalTarget;
        private ModelComponent target;

        public bool IsValid => 
            target == null || 
            Entity == null || 
            (target.Entity.Id != Entity.Id 
            && RecurseCheckChildren(Entity.Transform.Children, target.Entity.Transform)
            && CheckParent(target.Entity.Transform)
            );

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
            get { return target; }
            set
            {
                internalTarget = value;
                target = value;
                if (!IsValid)
                {
                    target = null;
                }
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

        private bool CheckParent(TransformComponent targetTransform)
        {
            var parent = targetTransform.Parent;
            while (parent != null)
            {
                if (targetTransform.Entity.Id == parent.Entity.Id)
                {
                    return false;
                }

                parent = parent.Parent;
            }

            return true;
        }

        private bool RecurseCheckChildren(FastCollection<TransformComponent> children, TransformComponent targetTransform)
        {
            foreach (var transformComponentChild in children)
            {
                if (transformComponentChild.Parent == null) continue; // skip this case, the parent has not updated it's list yet

                if (!RecurseCheckChildren(transformComponentChild.Children, targetTransform))
                    return false;

                if (targetTransform.Entity.Id != transformComponentChild.Entity.Id)
                    continue;

                return false;
            }
            return true;
        }


        internal void OnHierarchyChanged(object sender, Entity entity)
        {
            //currently this won't work with refs because the editor internally is using entity clones so ref comparison is useless
            if (entity == null || entity.Id != internalTarget?.Entity.Id) return;

            //possibly now it is fine to have a link
            Target = internalTarget;
        }
    }
}