// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// State stored in a <see cref="RenderPipeline"/> by a <see cref="ModelRenderer"/>
    /// </summary>
    internal class ModelRendererState
    {
        #region Constants and Fields

        public static PropertyKey<ModelRendererState> Key = new PropertyKey<ModelRendererState>("ModelRendererState", typeof(ModelRendererState));

        private readonly Dictionary<RenderPass, int> modelSlotMapping = new Dictionary<RenderPass, int>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRendererState"/> class.
        /// </summary>
        public ModelRendererState()
        {
            RenderModels = new List<RenderModel>();
        }

        /// <summary>
        /// Gets the mesh pass slot count.
        /// </summary>
        /// <value>The mesh pass slot count.</value>
        public int ModelSlotCount
        {
            get
            {
                return modelSlotMapping.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return AcceptModel != null && PrepareRenderModel != null && AcceptRenderModel != null;
            }
        }

        /// <summary>
        /// The action that will be applied on every model to test whether to add it to the render pipeline.
        /// </summary>
        public Func<IModelInstance, bool> AcceptModel { get; set; }

        /// <summary>
        /// Gets or sets the prepare render model.
        /// </summary>
        /// <value>The prepare render model.</value>
        public Action<RenderModel> PrepareRenderModel { get; set; }

        /// <summary>
        /// The action that will be applied on every render model to check 
        /// </summary>
        /// <value>The process mesh.</value>
        public Func<RenderModel, bool> AcceptRenderModel { get; set; }

        /// <summary>
        /// Gets the current list of models to render.
        /// </summary>
        /// <value>The render models.</value>
        public List<RenderModel> RenderModels { get; private set; }

        /// <summary>
        /// Gets or creates a mesh pass slot for this pass inside its <see cref="RenderPipeline" />.
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <returns>A mesh pass slot.</returns>
        public int GetModelSlot(RenderPass renderPass)
        {
            int meshPassSlot;
            if (!modelSlotMapping.TryGetValue(renderPass, out meshPassSlot))
            {
                modelSlotMapping[renderPass] = meshPassSlot = modelSlotMapping.Count;
            }
            return meshPassSlot;
        }
    }
}