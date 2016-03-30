// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Updater;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Add a <see cref="Model"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("ModelComponent")]
    [Display("Model", Expand = ExpandRule.Once)]
    // TODO GRAPHICS REFACTOR
    [DefaultEntityComponentProcessor(typeof(ModelTransformProcessor))]
    [DefaultEntityComponentRenderer(typeof(ModelRenderProcessor))]
    [ComponentOrder(11000)]
    public sealed class ModelComponent : ActivableEntityComponent, IModelInstance
    {
        private Model model;
        private SkeletonUpdater skeleton;
        private bool modelViewHierarchyDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponent"/> class.
        /// </summary>
        public ModelComponent() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelComponent"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelComponent(Model model)
        {
            Model = model;
            IsShadowCaster = true;
            IsShadowReceiver = true;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        /// <userdoc>The reference to the model asset to attach to this entity</userdoc>
        [DataMemberCustomSerializer]
        [DataMember(10)]
        public Model Model
        {
            get
            {
                return model;
            }
            set
            {
                if (model != value)
                    modelViewHierarchyDirty = true;
                model = value;
            }
        }

        /// <summary>
        /// Gets the materials; non-null ones will override materials from <see cref="SiliconStudio.Xenko.Rendering.Model.Materials"/> (same slots should be used).
        /// </summary>
        /// <value>
        /// The materials overriding <see cref="SiliconStudio.Xenko.Rendering.Model.Materials"/> ones.
        /// </value>
        /// <userdoc>The list of materials to use with the model. This list overrides the default materials of the model.</userdoc>
        [DataMember(20)]
        [Category]
        public List<Material> Materials { get; } = new List<Material>();

        [DataMemberIgnore, DataMemberUpdatable]
        [DataMember]
        public SkeletonUpdater Skeleton
        {
            get
            {
                CheckSkeleton();
                return skeleton;
            }
        }

        private void CheckSkeleton()
        {
            if (modelViewHierarchyDirty)
            {
                ModelUpdated();
                modelViewHierarchyDirty = false;
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating if this model component is casting shadows.
        /// </summary>
        /// <value>A boolean indicating if this model component is casting shadows.</value>
        /// <userdoc>If checked, the model generates a shadow when enabling shadow maps.</userdoc>
        [DataMember(30)]
        [DefaultValue(true)]
        [Display("Cast Shadows?")]
        public bool IsShadowCaster { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this model component is receiving shadows.
        /// </summary>
        /// <value>A boolean indicating if this model component is receiving shadows.</value>
        /// <userdoc>If checked, the model can be covered by the shadow of another model.</userdoc>
        [DataMember(40)]
        [DefaultValue(true)]
        [Display("Receive Shadows?")]
        public bool IsShadowReceiver { get; set; }

        /// <summary>
        /// Gets the bounding box in world space.
        /// </summary>
        /// <value>The bounding box.</value>
        [DataMemberIgnore]
        public BoundingBox BoundingBox;

        /// <summary>
        /// Gets the bounding sphere in world space.
        /// </summary>
        /// <value>The bounding sphere.</value>
        [DataMemberIgnore]
        public BoundingSphere BoundingSphere;

        /// <summary>
        /// Gets the material at the specified index. If the material is not overriden by this component, it will try to get it from <see cref="SiliconStudio.Xenko.Rendering.Model.Materials"/>
        /// </summary>
        /// <param name="index">The index of the material</param>
        /// <returns>The material at the specified index or null if not found</returns>
        public Material GetMaterial(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "index cannot be < 0");

            Material material = null;
            if (index < Materials.Count)
            {
                material = Materials[index];
            }
            if (material == null && Model != null && index < Model.Materials.Count)
            {
                material = Model.Materials[index].Material;
            }
            return material;
        }

        /// <summary>
        /// Gets the number of materials (computed from <see cref="SiliconStudio.Xenko.Rendering.Model.Materials"/>)
        /// </summary>
        /// <returns></returns>
        public int GetMaterialCount()
        {
            if (Model != null)
            {
                return Model.Materials.Count;
            }
            return 0;
        }

        private void ModelUpdated()
        {
            if (model != null)
            {
                if (skeleton != null)
                {
                    // Reuse previous ModelViewHierarchy
                    skeleton.Initialize(model.Skeleton);
                }
                else
                {
                    skeleton = new SkeletonUpdater(model.Skeleton);
                }
            }
        }

        internal void Update(TransformComponent transformComponent, ref Matrix worldMatrix)
        {
            if (!Enabled || model == null)
                return;

            // Check if scaling is negative
            bool isScalingNegative = false;
            {
                Vector3 scale, translation;
                Matrix rotation;
                if (worldMatrix.Decompose(out scale, out rotation, out translation))
                    isScalingNegative = scale.X*scale.Y*scale.Z < 0.0f;
            }

            // Make sure skeleton is up to date
            CheckSkeleton();

            if (skeleton != null)
            {
                // Update model view hierarchy node matrices
                skeleton.NodeTransformations[0].LocalMatrix = worldMatrix;
                skeleton.NodeTransformations[0].IsScalingNegative = isScalingNegative;
                skeleton.UpdateMatrices();
            }

            // Update the bounding sphere / bounding box in world space
            var meshes = Model.Meshes;
            var modelBoundingSphere = BoundingSphere.Empty;
            var modelBoundingBox = BoundingBox.Empty;
            bool hasBoundingBox = false;
            Matrix world;
            foreach (var mesh in meshes)
            {
                var meshBoundingSphere = mesh.BoundingSphere;

                if (skeleton != null)
                    skeleton.GetWorldMatrix(mesh.NodeIndex, out world);
                else
                    world = worldMatrix;
                Vector3.TransformCoordinate(ref meshBoundingSphere.Center, ref world, out meshBoundingSphere.Center);
                BoundingSphere.Merge(ref modelBoundingSphere, ref meshBoundingSphere, out modelBoundingSphere);

                var boxExt = new BoundingBoxExt(mesh.BoundingBox);
                boxExt.Transform(world);
                var meshBox = (BoundingBox)boxExt;

                if (hasBoundingBox)
                {
                    BoundingBox.Merge(ref modelBoundingBox, ref meshBox, out modelBoundingBox);
                }
                else
                {
                    modelBoundingBox = meshBox;
                }

                hasBoundingBox = true;
            }

            // Update the bounds
            BoundingBox = modelBoundingBox;
            BoundingSphere = modelBoundingSphere;
        }
    }
}