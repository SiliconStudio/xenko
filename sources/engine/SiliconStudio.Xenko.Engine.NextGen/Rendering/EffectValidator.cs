using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.IL;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// How to use:
    /// BeginEffectValidation();
    /// ValidateParameter(key1, value1);
    /// ValidateParameter(key2, value2);
    /// ...
    /// EndEffectValidation(); //returns true if same as last time, false if something changed
    /// You can use EffectValues to actually compile the effect.
    /// </summary>
    public struct EffectValidator
    {
        internal FastListStruct<EffectParameterEntry> EffectValues;
        private int effectValuesValidated; // This is used when validating
        private bool effectChanged;

        public void Initialize()
        {
            EffectValues = new FastListStruct<EffectParameterEntry>(4);
        }

        public void BeginEffectValidation()
        {
            effectValuesValidated = 0;
            effectChanged = false;
        }

        [RemoveInitLocals]
        public void ValidateParameter<T>(ParameterKey<T> key, T value)
        {
            // Check if value was existing and/or same
            var index = effectValuesValidated++;
            if (index < EffectValues.Count)
            {
                var currentEffectValue = EffectValues[index];
                if (currentEffectValue.Key == key && EqualityComparer<T>.Default.Equals((T)currentEffectValue.Value, value))
                {
                    // Everything same, let's keep going
                    return;
                }

                // Something was different, let's replace item and clear end of list
                EffectValues[index] = new EffectParameterEntry(key, value);
                EffectValues.Count = effectValuesValidated;
                effectChanged = true;
            }
            else
            {
                EffectValues.Add(new EffectParameterEntry(key, value));
                effectChanged = true;
            }
        }

        public bool EndEffectValidation()
        {
            if (effectValuesValidated < EffectValues.Count)
            {
                // Erase extra values
                EffectValues.Count = effectValuesValidated;
                return false;
            }

            return !effectChanged && effectValuesValidated == EffectValues.Count;
        }

        internal struct EffectParameterEntry
        {
            public readonly ParameterKey Key;
            public readonly object Value;

            public EffectParameterEntry(ParameterKey key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}