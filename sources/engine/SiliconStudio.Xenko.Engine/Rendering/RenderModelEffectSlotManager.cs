// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Handles <see cref="RenderModel"/> used by <see cref="ModelComponentRenderer"/>.
    /// </summary>
    internal class RenderModelEffectSlotManager
    {
        private readonly Dictionary<string, RenderModelEffectSlot> effectNameToSlot = new Dictionary<string, RenderModelEffectSlot>();
        private readonly Queue<RenderModelEffectSlot> availableSlots = new Queue<RenderModelEffectSlot>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderModelEffectSlotManager"/> class.
        /// </summary>
        public RenderModelEffectSlotManager()
        {
        }

        /// <summary>
        /// Gets or creates a slot for the specified effect name.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <returns>RenderModelEffectSlot.</returns>
        internal int AllocateSlot(string effectName)
        {
            RenderModelEffectSlot renderModelEffectSlot;
            if (!effectNameToSlot.TryGetValue(effectName, out renderModelEffectSlot))
            {
                // First, check free list, otherwise create a new slot
                if (availableSlots.Count > 0)
                {
                    renderModelEffectSlot = availableSlots.Dequeue();
                    renderModelEffectSlot.EffectName = effectName;
                    effectNameToSlot[effectName] = renderModelEffectSlot;
                }
                else
                    effectNameToSlot[effectName] = renderModelEffectSlot = new RenderModelEffectSlot(effectName, effectNameToSlot.Count);
            }

            renderModelEffectSlot.ReferenceCount++;
            return renderModelEffectSlot.Slot;
        }

        internal void ReleaseSlot(int slotIndex)
        {
            RenderModelEffectSlot selectedSlot = null;
            foreach (var slot in effectNameToSlot)
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
                if (!effectNameToSlot.Remove(selectedSlot.EffectName))
                    throw new InvalidOperationException("Model slot not found while trying to remove it.");
                availableSlots.Enqueue(selectedSlot);
            }
        }
    }

    internal class RenderModelEffectSlot
    {
        internal string EffectName;
        public int Slot;
        public int ReferenceCount;

        internal RenderModelEffectSlot(string effectName, int slot)
        {
            EffectName = effectName;
            Slot = slot;
        }
    }
}