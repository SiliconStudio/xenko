// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// State stored in a <see cref="RenderPipeline"/> by a <see cref="ModelComponentRenderer"/>
    /// </summary>
    internal class ModelRendererState
    {
        #region Constants and Fields

        public static PropertyKey<ModelRendererState> Key = new PropertyKey<ModelRendererState>("ModelRendererState", typeof(ModelRendererState));

        private readonly Dictionary<string, ModelRendererSlot> modelSlotMapping = new Dictionary<string, ModelRendererSlot>();
        private readonly Queue<ModelRendererSlot> availableModelSlots = new Queue<ModelRendererSlot>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRendererState"/> class.
        /// </summary>
        public ModelRendererState()
        {
        }

        public Dictionary<string, ModelRendererSlot> ModelSlotMapping
        {
            get { return modelSlotMapping; }
        }

        /// <summary>
        /// Gets or creates a mesh pass slot for this pass inside its <see cref="RenderPipeline" />.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <returns>ModelRendererSlot.</returns>
        public int AllocateModelSlot(string effectName)
        {
            ModelRendererSlot modelRendererSlot;
            if (!modelSlotMapping.TryGetValue(effectName, out modelRendererSlot))
            {
                // First, check free list, otherwise create a new slot
                if (availableModelSlots.Count > 0)
                {
                    modelRendererSlot = availableModelSlots.Dequeue();
                    modelRendererSlot.EffectName = effectName;
                    modelSlotMapping[effectName] = modelRendererSlot;
                }
                else
                    modelSlotMapping[effectName] = modelRendererSlot = new ModelRendererSlot(effectName, modelSlotMapping.Count);
            }

            modelRendererSlot.ReferenceCount++;
            return modelRendererSlot.Slot;
        }

        public void ReleaseModelSlot(int slotIndex)
        {
            ModelRendererSlot selectedSlot = null;
            foreach (var slot in modelSlotMapping)
            {
                if (slot.Value.Slot == slotIndex)
                {
                    selectedSlot = slot.Value;
                    break;
                }
            }

            if (selectedSlot == null)
            {
                throw new ArgumentOutOfRangeException("slotIndex", "Invalid slot");
            }

            if (--selectedSlot.ReferenceCount == 0)
            {
                // Release the slot if no other reference points to it (add it in free list)
                if (!modelSlotMapping.Remove(selectedSlot.EffectName))
                    throw new InvalidOperationException("Model slot not found while trying to remove it.");
                availableModelSlots.Enqueue(selectedSlot);
            }
        }
    }

    internal class ModelRendererSlot
    {
        internal string EffectName;
        public int Slot;
        public int ReferenceCount;

        internal ModelRendererSlot(string effectName, int slot)
        {
            EffectName = effectName;
            Slot = slot;
        }
    }
}