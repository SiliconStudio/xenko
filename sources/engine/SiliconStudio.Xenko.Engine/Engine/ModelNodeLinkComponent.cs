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

        [DataMemberIgnore]
        public bool IsValid { get; private set; }

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
                ValidityCheck(value);
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

        public void ValidityCheck()
        {
            ValidityCheck(target);
        }

        public void ValidityCheck(ModelComponent targetToValidate)
        {
            IsValid = targetToValidate == null ||
                        Entity == null ||
                        targetToValidate.Entity == null ||
                        (targetToValidate.Entity.Id != Entity.Id
                        && RecurseCheckChildren(Entity.Transform.Children, targetToValidate.Entity.Transform)
                        && CheckParent(targetToValidate.Entity.Transform)
                        );
        }

        internal void OnHierarchyChanged(object sender, Entity entity)
        {
            if (entity == null || entity.Id != Target?.Entity.Id) return;
            ValidityCheck();
        }

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
    }
}