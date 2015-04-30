// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a <see cref="Model"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("ModelComponent")]
    [Display(110, "Model")]
    [DefaultEntityComponentRenderer(typeof(ModelComponentAndPickingRenderer))]
    [DefaultEntityComponentProcessor(typeof(ModelProcessor))]
    public sealed class ModelComponent : EntityComponent, IModelInstance
    {
        public static PropertyKey<ModelComponent> Key = new PropertyKey<ModelComponent>("Key", typeof(ModelComponent));

        private Model model;
        private ModelViewHierarchyUpdater modelViewHierarchy;
        private bool modelViewHierarchyDirty = true;
        private List<Material> materials = new List<Material>();

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
            Parameters = new ParameterCollection();
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
        /// Gets the materials; non-null ones will override materials from <see cref="SiliconStudio.Paradox.Rendering.Model.Materials"/> (same slots should be used).
        /// </summary>
        /// <value>
        /// The materials overriding <see cref="SiliconStudio.Paradox.Rendering.Model.Materials"/> ones.
        /// </value>
        [DataMember(20)]
        public List<Material> Materials
        {
            get { return materials; }
        }

        [DataMemberIgnore]
        public ModelViewHierarchyUpdater ModelViewHierarchy
        {
            get
            {
                if (modelViewHierarchyDirty)
                {
                    ModelUpdated();
                    modelViewHierarchyDirty = false;
                }

                return modelViewHierarchy;
            }
        }

        /// <summary>
        /// Gets or sets the draw order (from lowest to highest).
        /// </summary>
        /// <value>
        /// The draw order.
        /// </value>
        [DataMember(30)]
        public float DrawOrder { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this model component is casting shadows.
        /// </summary>
        /// <value>A boolean indicating if this model component is casting shadows.</value>
        [DataMember(40)]
        [DefaultValue(true)]
        [Display("Cast Shadows?")]
        public bool IsShadowCaster { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this model component is receiving shadows.
        /// </summary>
        /// <value>A boolean indicating if this model component is receiving shadows.</value>
        [DataMember(40)]
        [DefaultValue(true)]
        [Display("Receive Shadows?")]
        public bool IsShadowReceiver { get; set; }

        /// <summary>
        /// Gets the parameters used to render this mesh.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMember(50)]
        public ParameterCollection Parameters { get; private set; }

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

        private void ModelUpdated()
        {
            if (model != null)
            {
                if (modelViewHierarchy != null)
                {
                    // Reuse previous ModelViewHierarchy
                    modelViewHierarchy.Initialize(model);
                }
                else
                {
                    modelViewHierarchy = new ModelViewHierarchyUpdater(model);
                }
            }
        }

        internal void Update(ref Matrix worldMatrix)
        {
            // Update model view hierarchy node matrices
            modelViewHierarchy.NodeTransformations[0].LocalMatrix = worldMatrix;
            modelViewHierarchy.UpdateMatrices();

            // Update the bounding sphere / bounding box in world space
            var meshes = Model.Meshes;
            var modelBoundingSphere = BoundingSphere.Empty;
            var modelBoundingBox = BoundingBox.Empty;
            bool hasBoundingBox = false;
            Matrix world;
            foreach (var mesh in meshes)
            {
                var meshBoundingSphere = mesh.BoundingSphere;

                modelViewHierarchy.GetWorldMatrix(mesh.NodeIndex, out world);
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

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}