// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Paradox.Effects
{
    internal class EffectParameterUpdaterDefinition : ParameterUpdaterDefinition
    {
        internal object[] SortedCompilationValues;

        internal int[] SortedCounters;

        internal int[] SortedLevels;

        public EffectParameterUpdaterDefinition(ParameterCollection parameters)
        {
            var internalValues = parameters.InternalValues;
            SortedKeys = new ParameterKey[internalValues.Count];
            SortedKeyHashes = new ulong[internalValues.Count];
            SortedCompilationValues = new object[internalValues.Count];
            SortedCounters = new int[internalValues.Count];

            for (int i = 0; i < internalValues.Count; ++i)
            {
                var internalValue = internalValues[i];

                SortedKeys[i] = internalValue.Key;
                SortedKeyHashes[i] = internalValue.Key.HashCode;
                SortedCompilationValues[i] = internalValue.Value.Object;
                SortedCounters[i] = internalValue.Value.Counter;
            }
        }

        public void UpdateCounter(ParameterCollection parameters)
        {
            var internalValues = parameters.InternalValues;
            for (int i = 0; i < internalValues.Count; ++i)
            {
                var internalValue = internalValues[i];

                SortedCounters[i] = internalValue.Value.Counter;
            }
        }
    }

    internal class EffectParameterUpdater : ParameterUpdater
    {
        public KeyValuePair<int, ParameterCollection.InternalValue> GetAtIndex(int index)
        {
            return InternalValues[index];
        }
    }
}
