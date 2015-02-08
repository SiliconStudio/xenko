// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Used to detect parameters change for dynamic effect.
    /// </summary>
    internal class EffectParameterUpdaterDefinition : ParameterUpdaterDefinition
    {
        internal object[] SortedCompilationValues;

        internal int[] SortedCounters;

        internal int[] SortedLevels;

        public EffectParameterUpdaterDefinition(Effect effect)
        {
            Initialize(effect);
        }

        public void Initialize(Effect effect)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            var parameters = effect.CompilationParameters;

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

            var keyMapping = new Dictionary<ParameterKey, int>();
            for (int i = 0; i < SortedKeys.Length; i++)
                keyMapping.Add(SortedKeys[i], i);
            effect.CompilationParameters.SetKeyMapping(keyMapping);
            effect.DefaultCompilationParameters.SetKeyMapping(keyMapping);
        }

        public void UpdateCounter(ParameterCollection parameters)
        {
            var internalValues = parameters.InternalValues;
            for (int i = 0; i < internalValues.Count; ++i)
            {
                SortedCounters[i] = internalValues[i].Value.Counter;
            }
        }
    }

    internal class EffectParameterUpdater : ParameterUpdater
    {
        public KeyValuePair<int, ParameterCollection.InternalValue> GetAtIndex(int index)
        {
            return InternalValues[index];
        }

        public bool HasChanged(EffectParameterUpdaterDefinition definition)
        {
            for (var i = 0; i < definition.SortedLevels.Length; ++i)
            {
                var kvp = GetAtIndex(i);
                if (definition.SortedLevels[i] == kvp.Key)
                {
                    if (definition.SortedCounters[i] != kvp.Value.Counter && !Equals(definition.SortedCompilationValues[i], kvp.Value.Object))
                        return true;
                }
                else
                {
                    if (!Equals(definition.SortedCompilationValues[i], kvp.Value.Object))
                        return true;
                }
            }
            return false;          
        }

        public void ComputeLevels(EffectParameterUpdaterDefinition definition)
        {
            var levels = definition.SortedLevels;
            if (levels == null || levels.Length != definition.SortedKeyHashes.Length)
            {
                levels = new int[definition.SortedKeyHashes.Length];
            }

            for (var i = 0; i < levels.Length; ++i)
            {
                levels[i] = GetAtIndex(i).Key;
            }

            definition.SortedLevels = levels;            
        }

        public void UpdateCounters(EffectParameterUpdaterDefinition definition)
        {
            for (var i = 0; i < definition.SortedLevels.Length; ++i)
            {
                var kvp = GetAtIndex(i);
                definition.SortedCounters[i] = kvp.Value.Counter;
            }
        }



    }
}
