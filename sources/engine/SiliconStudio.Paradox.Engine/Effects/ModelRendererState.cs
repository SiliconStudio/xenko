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

        private readonly Dictionary<SlotKey, int> modelSlotMapping = new Dictionary<SlotKey, int>();

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
                return AcceptModel != null && PrepareRenderModel != null;
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
        /// Gets the current list of models to render.
        /// </summary>
        /// <value>The render models.</value>
        public List<RenderModel> RenderModels { get; private set; }

        /// <summary>
        /// Gets or creates a mesh pass slot for this pass inside its <see cref="RenderPipeline" />.
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <returns>A mesh pass slot.</returns>
        public int GetModelSlot(RenderPass renderPass, string effectName)
        {
            int meshPassSlot;
            var key = new SlotKey(renderPass, effectName);
            if (!modelSlotMapping.TryGetValue(key, out meshPassSlot))
            {
                modelSlotMapping[key] = meshPassSlot = modelSlotMapping.Count;
            }
            return meshPassSlot;
        }

        private struct SlotKey : IEquatable<SlotKey>
        {
            public SlotKey(RenderPass pass, string effectName)
            {
                Pass = pass;
                EffectName = effectName;
            }

            public readonly RenderPass Pass;

            public readonly string EffectName;

            public bool Equals(SlotKey other)
            {
                return string.Equals(EffectName, other.EffectName) && Equals(Pass, other.Pass);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is SlotKey && Equals((SlotKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((EffectName != null ? EffectName.GetHashCode() : 0) * 397) ^ (Pass != null ? Pass.GetHashCode() : 0);
                }
            }
        }
    }
}