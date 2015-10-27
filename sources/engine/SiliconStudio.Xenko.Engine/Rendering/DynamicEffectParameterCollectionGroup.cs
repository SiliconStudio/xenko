// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering
{
    internal class DynamicEffectParameterCollectionGroup : ParameterCollectionGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEffectParameterCollectionGroup"/> class.
        /// </summary>
        /// <param name="parameterCollections">The parameter collections.</param>
        public DynamicEffectParameterCollectionGroup(ParameterCollection[] parameterCollections) : base(parameterCollections)
        {
        }

        public ParameterCollection[] ParameterCollections
        {
            get { return parameterCollections; }
        }

        public BoundInternalValue GetAtIndex(int index)
        {
            return InternalValues[index];
        }

        public bool HasChanged(DynamicEffectParameterUpdaterDefinition definition)
        {
            for (var i = 0; i < definition.SortedLevels.Length; ++i)
            {
                var kvp = GetAtIndex(i);

                // We can skip keys defined by the first level (which is the effect default parameters + key mapping)
                // TODO: Make sure this is a valid assumption in all cases
                if (kvp.DirtyCount == 0)
                    continue;

                if (definition.SortedLevels[i] == kvp.DirtyCount)
                {
                    if (definition.SortedCounters[i] != kvp.Value.Counter && !kvp.Value.ValueEquals(definition.SortedCompilationValues[i]))
                        return true;
                }
                else
                {
                    if (!kvp.Value.ValueEquals(definition.SortedCompilationValues[i]))
                        return true;
                }
            }
            return false;          
        }

        public void ComputeLevels(DynamicEffectParameterUpdaterDefinition definition)
        {
            var levels = definition.SortedLevels;
            if (levels == null || levels.Length != definition.SortedKeyHashes.Length)
            {
                levels = new int[definition.SortedKeyHashes.Length];
            }

            for (var i = 0; i < levels.Length; ++i)
            {
                levels[i] = GetAtIndex(i).DirtyCount;
            }

            definition.SortedLevels = levels;            
        }

        public void UpdateCounters(DynamicEffectParameterUpdaterDefinition definition)
        {
            for (var i = 0; i < definition.SortedLevels.Length; ++i)
            {
                var kvp = GetAtIndex(i);
                definition.SortedCounters[i] = kvp.Value.Counter;
            }
        }



    }
}
