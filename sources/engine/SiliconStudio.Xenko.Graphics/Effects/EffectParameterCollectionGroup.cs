// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Graphics.Internals
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

        public EffectParameterCollectionGroup(GraphicsDevice graphicsDevice, Effect effect, int count, ParameterCollection[] values)
            : base(CreateParameterCollections(graphicsDevice, effect, count, values))
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (effect == null) throw new ArgumentNullException("effect");
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

        private static ParameterCollection[] CreateParameterCollections(GraphicsDevice graphicsDevice, Effect effect, int count, ParameterCollection[] parameterCollections)
        {
            var result = new ParameterCollection[2 + count];
            result[0] = effect.DefaultParameters;
            for (int i = 0; i < count; ++i)
            {
                result[i + 1] = parameterCollections[i];
            }
            result[count + 1] = graphicsDevice.Parameters;

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