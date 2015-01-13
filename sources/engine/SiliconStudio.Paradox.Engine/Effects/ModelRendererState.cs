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

        private readonly Dictionary<SlotKey, ModelRendererSlot> modelSlotMapping = new Dictionary<SlotKey, ModelRendererSlot>();
        private readonly Queue<ModelRendererSlot> availableModelSlots = new Queue<ModelRendererSlot>();

        #endregion

        public event Action<ModelRendererState, ModelRendererSlot> ModelSlotAdded;
        public event Action<ModelRendererState, ModelRendererSlot> ModelSlotRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRendererState"/> class.
        /// </summary>
        public ModelRendererState()
        {
            RenderModels = new List<RenderModel>();
        }

        public Dictionary<SlotKey, ModelRendererSlot> ModelSlotMapping
        {
            get { return modelSlotMapping; }
        }

        /// <summary>
        /// The action that will be applied on every model to test whether to add it to the render pipeline.
        /// </summary>
        public Func<IModelInstance, bool> AcceptModel { get; set; }

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
        /// <param name="prepareRenderModel">The prepare render model.</param>
        /// <param name="modelRendererSlot">The model renderer slot.</param>
        public void AllocateModelSlot(RenderPass renderPass, string effectName, Action<RenderModel> prepareRenderModel, out ModelRendererSlot modelRendererSlot)
        {
            var key = new SlotKey(renderPass, effectName);
            if (!modelSlotMapping.TryGetValue(key, out modelRendererSlot))
            {
                // First, check free list, otherwise create a new slot
                if (availableModelSlots.Count > 0)
                {
                    modelRendererSlot = availableModelSlots.Dequeue();
                    modelRendererSlot.Key = key;
                    modelSlotMapping[key] = modelRendererSlot;
                }
                else
                    modelSlotMapping[key] = modelRendererSlot = new ModelRendererSlot(key, modelSlotMapping.Count);

                modelRendererSlot.PrepareRenderModel = prepareRenderModel;

                if (ModelSlotAdded != null)
                    ModelSlotAdded(this, modelRendererSlot);
            }

            modelRendererSlot.ReferenceCount++;
        }

        public void ReleaseModelSlot(ModelRendererSlot modelRendererSlot)
        {
            if (--modelRendererSlot.ReferenceCount == 0)
            {
                if (ModelSlotRemoved != null)
                    ModelSlotRemoved(this, modelRendererSlot);

                // Release the slot if no other reference points to it (add it in free list)
                if (!modelSlotMapping.Remove(modelRendererSlot.Key))
                    throw new InvalidOperationException("Model slot not found while trying to remove it.");
                modelRendererSlot.PrepareRenderModel = null;
                availableModelSlots.Enqueue(modelRendererSlot);
            }
        }

        internal struct SlotKey : IEquatable<SlotKey>
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

    internal class ModelRendererSlot
    {
        internal ModelRendererState.SlotKey Key;
        public Action<RenderModel> PrepareRenderModel;
        public int Slot;
        public int ReferenceCount;

        internal ModelRendererSlot(ModelRendererState.SlotKey key, int slot)
        {
            Key = key;
            Slot = slot;
        }
    }
}