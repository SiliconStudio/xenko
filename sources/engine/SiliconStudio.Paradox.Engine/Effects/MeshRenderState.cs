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
    public class MeshRenderState
    {
        #region Constants and Fields

        public static PropertyKey<MeshRenderState> Key = new PropertyKey<MeshRenderState>("MeshRenderState", typeof(MeshRenderState));

        private readonly Dictionary<RenderPass, int> meshPassSlotMapping = new Dictionary<RenderPass, int>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshRenderState"/> class.
        /// </summary>
        public MeshRenderState()
        {
            RenderModels = new List<RenderModel>();
        }

        /// <summary>
        /// Gets the mesh pass slot count.
        /// </summary>
        /// <value>The mesh pass slot count.</value>
        public int MeshPassSlotCount
        {
            get
            {
                return meshPassSlotMapping.Count;
            }
        }

        /// <summary>
        /// The action that will be applied on every mesh instantiated in this render pipeline.
        /// </summary>
        /// <value>The process mesh.</value>
        public Action<RenderModel, ParameterCollection> PrepareRenderModel { get; set; }

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
        public int GetMeshPassSlot(RenderPass renderPass)
        {
            int meshPassSlot;
            if (!meshPassSlotMapping.TryGetValue(renderPass, out meshPassSlot))
            {
                meshPassSlotMapping[renderPass] = meshPassSlot = meshPassSlotMapping.Count;
            }
            return meshPassSlot;
        }
    }
}