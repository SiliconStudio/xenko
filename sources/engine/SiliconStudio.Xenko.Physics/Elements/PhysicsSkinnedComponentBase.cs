// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract("PhysicsSkinnedComponentBase")]
    [Display("PhysicsSkinnedComponentBase")]
    public abstract class PhysicsSkinnedComponentBase : PhysicsTriggerComponentBase
    {
        /// <summary>
        /// Gets or sets the link (usually a bone).
        /// </summary>
        /// <value>
        /// The mesh's linked bone name
        /// </value>
        /// <userdoc>
        /// In the case of skinned mesh this must be the bone node name linked with this element. Cannot change during run-time.
        /// </userdoc>
        [DataMember(190)]
        public string NodeName { get; set; }


        protected override void OnAttach()
        {
            base.OnAttach();

            if (NodeName.IsNullOrEmpty() || Data.ModelComponent?.Skeleton == null) return;

            if (!Data.BoneMatricesUpdated)
            {
                Vector3 position, scaling;
                Quaternion rotation;
                Entity.Transform.WorldMatrix.Decompose(out scaling, out rotation, out position);
                var isScalingNegative = scaling.X * scaling.Y * scaling.Z < 0.0f;
                Data.ModelComponent.Skeleton.NodeTransformations[0].LocalMatrix = Entity.Transform.WorldMatrix;
                Data.ModelComponent.Skeleton.NodeTransformations[0].IsScalingNegative = isScalingNegative;
                Data.ModelComponent.Skeleton.UpdateMatrices();
                Data.BoneMatricesUpdated = true;
            }

            BoneIndex = Data.ModelComponent.Skeleton.Nodes.IndexOf(x => x.Name == this.NodeName);

            if (BoneIndex == -1)
            {
                throw new InvalidOperationException("The specified NodeName doesn't exist in the model hierarchy.");
            }

            BoneWorldMatrixOut = BoneWorldMatrix = Data.ModelComponent.Skeleton.NodeTransformations[BoneIndex].WorldMatrix;
        }
    }
}