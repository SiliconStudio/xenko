// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A list of <see cref="LightComponentCollection"/> for a particular type of light (direct light, direct light + shadows, environment lights).
    /// </summary>
    public sealed class LightComponentCollectionGroup : IEnumerable<LightComponentCollection>
    {
        // The reason of this class is to store lights according to their culling mask while minimizing the number of LightComponentCollection needed
        // if culling mask are similar.
        // For example, suppose we have a list of lights per cullin mask:
        // 111000  -> Light1, Light2, Light3, Light4, Light5
        // 101000  -> Light6
        // 100000  -> Light7, Light8
        //
        // We will generate 3 different LightComponentCollection (#number is the LightComponentCollectionInstance):
        // 100000  -> #1 Light1, Light2, Light3, Light4, Light5, Light6, Light7, Light8
        // 101000  -> #2 Light1, Light2, Light3, Light4, Light5, Light6
        // 111000  -> #3 Light1, Light2, Light3, Light4, Light5
        //
        // But if all lights belong to the same mask like 111111
        // 111000  -> Light1, Light2, Light3, Light4, Light5, Light6, Light7, Light8
        // We will generate a single group:
        // 111000  -> #1 Light1, Light2, Light3, Light4, Light5, Light6, Light7, Light8
        //
        // We are then able to retreive a LightComponentCollection for a specific group

        private readonly LightComponentCollection[] lightsPool;
        private readonly List<LightComponentCollection> lights;
        private readonly List<LightComponent> allLights;
        private readonly uint[] groupMasks;
        private readonly List<int> activeCollections = new List<int>();

        private readonly List<EntityGroupMask> allMasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponentCollectionGroup"/> class.
        /// </summary>
        internal LightComponentCollectionGroup()
        {
            lightsPool = new LightComponentCollection[32];
            lights = new List<LightComponentCollection>();
            groupMasks = new uint[32 * 2];
            allLights = new List<LightComponent>();
            allMasks = new List<EntityGroupMask>();
        }

        /// <summary>
        /// Gets the <see cref="LightComponentCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>LightComponentCollection.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index [{0}] out of range [0, {1}].ToFormat(index, lights.Count - 1)</exception>
        public LightComponentCollection this[int index]
        {
            get
            {
                if (index < 0 || index > lights.Count - 1)
                    throw new ArgumentOutOfRangeException("index [{0}] out of range [0, {1}]".ToFormat(index, lights.Count - 1));
                return lightsPool[index];
            }
        }

        /// <summary>
        /// Gets the light affecting a specific group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>LightComponentCollection.</returns>
        public LightComponentCollection FindGroup(EntityGroup group)
        {
            int groupBaseIndex = (int)group << 1;
            if (groupMasks[groupBaseIndex] != 0)
            {
                return lightsPool[groupMasks[groupBaseIndex + 1]];
            }
            return null;
        }

        /// <summary>
        /// Gets all the lights stored in this group.
        /// </summary>
        /// <value>All lights.</value>
        public List<LightComponent> AllLights
        {
            get
            {
                return allLights;
            }
        }

        /// <summary>
        /// Gets the number of <see cref="LightComponentCollection"/> stored in this group.
        /// </summary>
        /// <value>The number of <see cref="LightComponentCollection"/> stored in this group.</value>
        public int Count
        {
            get
            {
                return lights.Count;
            }
        }

        internal unsafe void Clear()
        {
            allLights.Clear();
            allMasks.Clear();

            fixed (void* ptr = groupMasks)
                Interop.memset(ptr, 0, groupMasks.Length);
                
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].Clear();
            }
        }

        internal void PrepareLight(LightComponent lightComponent)
        {
            var cullingMask = lightComponent.CullingMask;

            // We expect we don't have a huge combination of culling mask, at least no more than the number of entities
            // TODO: switch to dictionary at runtime if there are too many masks
            if (allMasks.Contains(cullingMask))
            {
                return;
            }
            allMasks.Add(cullingMask);

            // Fit all masks and prepare collection group based on mask
            var groupMask = (uint)cullingMask;
            for (int groupIndex = 0; groupMask != 0; groupMask = groupMask >> 1, groupIndex++)
            {
                if ((groupMask & 1) == 0)
                {
                    continue;
                }

                var previousMask = groupMasks[groupIndex * 2];
                previousMask = previousMask == 0 ? (uint)cullingMask : previousMask & (uint)cullingMask;
                groupMasks[groupIndex * 2] = previousMask;
            }
        }

        internal void AllocateCollections()
        {
            // Iterate only on the maximum of group mask
            lights.Clear();
            for (int i = 0; i < groupMasks.Length;)
            {
                var mask = groupMasks[i++];
                if (mask == 0)
                {
                    continue;
                }

                int collectionIndex = -1;
                for (int j = 0; j < lights.Count; j++)
                {
                    if ((int)lightsPool[j].CullingMask == mask)
                    {
                        collectionIndex = j;
                        break;
                    }
                }
                if (collectionIndex < 0)
                {
                    collectionIndex = lights.Count;
                    var collection = lightsPool[collectionIndex];
                    if (collection == null)
                    {
                        lightsPool[collectionIndex] = collection = new LightComponentCollection();
                    }
                    lights.Add(collection);
                    collection.CullingMask = (EntityGroupMask)mask;
                }

                groupMasks[i++] = (uint)collectionIndex;
            }
        }

        internal void AddLight(LightComponent lightComponent)
        {
            var cullingMask = lightComponent.CullingMask;
            uint collectionMaskAdded = 0;

            // As we are checking all 32 bits, this may not be optimal. 
            // TODO: optimize this loop instead of iterating on all bits
            var groupMask = (uint)cullingMask;            
            for (int groupIndex = 1; groupMask != 0; groupMask = groupMask >> 1, groupIndex += 2)
            {
                if ((groupMask & 1) == 0)
                {
                    continue;
                }

                // Add the light to the proper collection
                uint collectionIndex = groupMasks[groupIndex];
                uint collectionIndexMask = 1u << (int)collectionIndex;
                if ((collectionMaskAdded & collectionIndexMask) == 0)
                {
                    collectionMaskAdded |= collectionIndexMask;
                    lightsPool[collectionIndex].Add(lightComponent);
                }
            }

            allLights.Add(lightComponent);
        }

        public List<Lights.LightComponentCollection>.Enumerator GetEnumerator()
        {
            return lights.GetEnumerator();
        }

        IEnumerator<LightComponentCollection> IEnumerable<LightComponentCollection>.GetEnumerator()
        {
            return lights.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lights.GetEnumerator();
        }
    }
}