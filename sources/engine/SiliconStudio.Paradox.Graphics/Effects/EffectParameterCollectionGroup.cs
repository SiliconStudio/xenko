// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    /// <summary>
    /// Updates ParameterCollection for rendering, including dynamic parameters.
    /// </summary>
    public class EffectParameterCollectionGroup : ParameterCollectionGroup
    {
        public Effect Effect { get; private set; }

        public EffectParameterCollectionGroup(GraphicsDevice graphicsDevice, Effect effect, IList<ParameterCollection> parameterCollections) : base(CreateParameterCollections(graphicsDevice, effect, parameterCollections))
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (effect == null) throw new ArgumentNullException("effect");
            if (parameterCollections == null) throw new ArgumentNullException("parameterCollections");

            Effect = effect;
        }

        private static ParameterCollection[] CreateParameterCollections(GraphicsDevice graphicsDevice, Effect effect, IList<ParameterCollection> parameterCollections)
        {
            var result = new ParameterCollection[2 + parameterCollections.Count];
            result[0] = effect.DefaultParameters;
            for (int i = 0; i < parameterCollections.Count; ++i)
            {
                result[i + 1] = parameterCollections[i];
            }
            result[parameterCollections.Count + 1] = graphicsDevice.Parameters;

            return result;
        }

        internal int InternalValueBinarySearch(int[] values, int keyHash)
        {
            int start = 0;
            int end = values.Length - 1;
            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var hash1 = values[middle];
                var hash2 = keyHash;

                if (hash1 == hash2)
                {
                    return middle;
                }
                if (hash1 < hash2)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }
            return ~start;
        }

        internal void Update(GraphicsDevice graphicsDevice, EffectParameterUpdaterDefinition definition)
        {
            Update(definition);

            // ----------------------------------------------------------------
            // Update dynamic values
            // Definitions based on Default+Pass lists
            // ----------------------------------------------------------------
            var dependencies = definition.Dependencies;
            for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; ++dependencyIndex)
            {
                var dependency = dependencies[dependencyIndex];
                int highestLevel = 0;
                int destinationLevel = InternalValues[dependency.Destination].DirtyCount;

                var destination = InternalValues[dependency.Destination];
                bool needUpdate = false;
                for (int i = 0; i < dependency.Sources.Length; ++i)
                {
                    var source = InternalValues[dependency.Sources[i]];
                    var sourceLevel = source.DirtyCount;

                    if (highestLevel < sourceLevel)
                        highestLevel = sourceLevel;
                }

                if (destinationLevel < highestLevel)
                {
                    // Dynamic value: the sources of this dynamic value are defined in a most derived collection than its destination collection
                    // as a result, we need to create or use a new dynamic value at the appropriate level.

                    // Find last collection index (excluding parameterOverrides)
                    int maxLevelNoOverride = parameterCollections.Length - 1;
                    if (highestLevel <= maxLevelNoOverride)
                    {
                        // TODO: Choose target level more properly (i.e. mesh*pass => meshpass)
                        // Sources all comes from normal collections => override in the most derived collection
                        // For now, use maxLevelNoOverride instead of highestLevel just to be safe.
                        var bestCollectionForDynamicValue = parameterCollections[maxLevelNoOverride];
                        var key = definition.SortedKeys[dependency.Destination];
                        bestCollectionForDynamicValue.SetDefault(key, true);
                        InternalValues[dependency.Destination] = destination = new BoundInternalValue(highestLevel, bestCollectionForDynamicValue.GetInternalValue(key));
                    }
                    else
                    {
                        // At least one source comes from TLS override, so use dynamic from TLS dynamic storage as well.
                        InternalValues[dependency.Destination] = destination = new BoundInternalValue(parameterCollections.Length, definition.ThreadLocalDynamicValues[graphicsDevice.ThreadIndex][dependencyIndex]);
                    }

                    needUpdate = true;
                }

                // Force updating (even if no parameters has been updated)
                if (!dependency.Dynamic.AutoCheckDependencies)
                    needUpdate = true;

                if (destination.Value.Dependencies == null || destination.Value.Dependencies.Length < dependency.Sources.Length)
                    destination.Value.Dependencies = new ParameterCollection.InternalValueReference[dependency.Sources.Length];

                var dependencyParameters = destination.Value.Dependencies;
                for (int i = 0; i < dependency.Sources.Length; ++i)
                {
                    var source = InternalValues[dependency.Sources[i]];
                    var internalValue = source.Value;

                    if (dependencyParameters[i].Entry != internalValue)
                    {
                        needUpdate = true;
                        dependencyParameters[i].Entry = internalValue;
                        dependencyParameters[i].Counter = internalValue.Counter;
                    }
                    else if (dependencyParameters[i].Counter != internalValue.Counter)
                    {
                        needUpdate = true;
                        dependencyParameters[i].Counter = internalValue.Counter;
                    }
                }

                if (needUpdate)
                {
                    // At least one source was updated
                    dependency.Dynamic.GetValue(destination.Value);
                    destination.Value.Counter++;
                }
            }
        }

        internal ParameterCollection.InternalValue GetInternalValue(int index)
        {
            return InternalValues[index].Value;
        }

        internal T GetValue<T>(int index)
        {
            return ((ParameterCollection.InternalValueBase<T>)InternalValues[index].Value).Value;
        }
    }
}