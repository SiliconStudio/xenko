// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Describes the expected parameters when using a <see cref="ParameterUpdater"/>.
    /// </summary>
    internal abstract class ParameterUpdaterDefinition
    {
        public ParameterKey[] SortedKeys { get; protected set; }

        public ulong[] SortedKeyHashes { get; protected set; }
    }

    /// <summary>
    /// Merges and filters parameters coming from multiple <see cref="ParameterCollection"/>.
    /// </summary>
    internal class ParameterUpdater
    {
        protected BoundInternalValue[] InternalValues;

        public ParameterUpdater()
        {
            InternalValues = new BoundInternalValue[8];
        }

        public void Update(ParameterUpdaterDefinition definition, ParameterCollection[] parameterCollections, int levelCount)
        {
            var sortedKeyHashes = definition.SortedKeyHashes;
            var sortedKeyHashesLength = sortedKeyHashes.Length;
            if (sortedKeyHashesLength == 0)
                return;

            if (sortedKeyHashesLength > InternalValues.Length)
                InternalValues = new BoundInternalValue[sortedKeyHashesLength];

            // Temporary clear data for debug/test purposes (shouldn't necessary)
            for (int i = 0; i < sortedKeyHashesLength; ++i)
                InternalValues[i] = new BoundInternalValue();

            // Optimization: List is already prepared (with previous SetKeyMapping() call)
            var collection = parameterCollections[0];
            var keys = collection.keys;
            for (int i = 0; i < keys.Length; ++i)
            {
                var internalValue = keys[i];
                if (internalValue != null)
                    InternalValues[i] = new BoundInternalValue(0, internalValue);
            }

            // Iterate over parameter collections
            for (int levelIndex = 1; levelIndex < levelCount; ++levelIndex)
            {
                var level = parameterCollections[levelIndex].valueList;
                int index = 0;
                var currentHash = sortedKeyHashes[index];
                int internalValueCount = level.Count;
                var items = level.Items;

                // Iterate over each items
                // Since both expected values and parameter collection values are sorted,
                // we iterate over both of them together so that we can easily detect what needs to be copied.
                // Note: We use a 64-bit hash that we assume has no collision (maybe we should
                //       detect unlikely collision when registering them?)
                for (int i = 0; i < internalValueCount; ++i)
                {
                    var internalValue = items[i];
                    var expectedHash = internalValue.Key.HashCode;
                    //index = InternalValueBinarySearch(sortedKeyHashes, internalValue.Key.GetHashCode());
                    while (currentHash < expectedHash)
                    {
                        if (++index >= sortedKeyHashesLength)
                            break;
                        currentHash = sortedKeyHashes[index];
                    }
                    if (currentHash == expectedHash)
                    {
                        // Update element
                        InternalValues[index] = new BoundInternalValue(levelIndex, internalValue.Value);
                    }
                }
            }
        }

        public object GetObject(int index)
        {
            return (InternalValues[index].Value).Object;
        }

        internal struct BoundInternalValue
        {
            public int DirtyCount;
            public ParameterCollection.InternalValue Value;

            public BoundInternalValue(int dirtyCount, ParameterCollection.InternalValue value)
            {
                DirtyCount = dirtyCount;
                Value = value;
            }
        }
    }
}
