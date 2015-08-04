// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Describes the expected parameters when using a <see cref="ParameterCollectionGroup"/>.
    /// </summary>
    internal abstract class ParameterUpdaterDefinition
    {
        public ParameterKey[] SortedKeys { get; protected set; }

        public ulong[] SortedKeyHashes { get; protected set; }
    }

    /// <summary>
    /// Merges and filters parameters coming from multiple <see cref="ParameterCollection"/>, using the first collection as key-mapping reference.
    /// </summary>
    public abstract class ParameterCollectionGroup
    {
        internal BoundInternalValue[] InternalValues;
        protected int[] previousParameterCollections;
        protected readonly ParameterCollection[] parameterCollections;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollectionGroup" /> class.
        /// </summary>
        /// <param name="parameterCollections">The parameter collections.</param>
        /// <exception cref="System.ArgumentNullException">parameterCollections</exception>
        protected ParameterCollectionGroup(ParameterCollection[] parameterCollections)
        {
            if (parameterCollections == null) throw new ArgumentNullException("parameterCollections");

            InternalValues = new BoundInternalValue[8];
            this.parameterCollections = parameterCollections;
        }

        internal void Update(ParameterUpdaterDefinition definition)
        {
            var sortedKeyHashes = definition.SortedKeyHashes;
            var sortedKeyHashesLength = sortedKeyHashes.Length;
            if (sortedKeyHashesLength == 0)
                return;

            if (sortedKeyHashesLength > InternalValues.Length)
                InternalValues = new BoundInternalValue[sortedKeyHashesLength];


            // Optimization: nothing changed?
            if (previousParameterCollections == null || previousParameterCollections.Length < parameterCollections.Length)
            {
                previousParameterCollections = new int[parameterCollections.Length];
            }

            bool needUpdate = false;
            for (int levelIndex = 0; levelIndex < parameterCollections.Length; ++levelIndex)
            {
                var parameterCollection = parameterCollections[levelIndex];
                var keyCounter = parameterCollection.KeyVersion;
                if (keyCounter != previousParameterCollections[levelIndex])
                {
                    needUpdate = true;
                    previousParameterCollections[levelIndex] = keyCounter;
                }
            }

            if (!needUpdate)
            {
                return;
            }

            // Temporary clear data for debug/test purposes (shouldn't necessary)
            for (int i = 0; i < sortedKeyHashesLength; ++i)
                InternalValues[i] = new BoundInternalValue();

            // Optimization: List is already prepared (with previous SetKeyMapping() call)
            var collection = parameterCollections[0];
            var keys = collection.IndexedInternalValues;
            for (int i = 0; i < keys.Length; ++i)
            {
                var internalValue = keys[i];
                if (internalValue != null)
                    InternalValues[i] = new BoundInternalValue(0, internalValue);
            }

            // Iterate over parameter collections
            for (int levelIndex = 1; levelIndex < parameterCollections.Length; ++levelIndex)
            {
                var level = parameterCollections[levelIndex].InternalValues;
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

        internal object GetObject(int index)
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
