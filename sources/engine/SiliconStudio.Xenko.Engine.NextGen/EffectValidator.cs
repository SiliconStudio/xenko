using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Rendering;

namespace RenderArchitecture
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
        internal FastListStruct<KeyValuePair<ParameterKey, object>> EffectValues;
        private int effectValuesValidated; // This is used when validating
        private bool effectChanged;

        public void Initialize()
        {
            EffectValues = new FastListStruct<KeyValuePair<ParameterKey, object>>(4);
        }

        public void BeginEffectValidation()
        {
            effectValuesValidated = 0;
            effectChanged = false;
        }

        public void ValidateParameter(ParameterKey key, object value)
        {
            // Check if value was existing and/or same
            var index = effectValuesValidated++;
            if (index < EffectValues.Count)
            {
                var currentEffectValue = EffectValues[index];
                if (currentEffectValue.Key == key && currentEffectValue.Value == value)
                {
                    // Everything same, let's keep going
                    return;
                }

                // Something was different, let's replace item and clear end of list
                EffectValues[index] = new KeyValuePair<ParameterKey, object>(key, value);
                EffectValues.Count = effectValuesValidated;
                effectChanged = true;
            }
            else
            {
                EffectValues.Add(new KeyValuePair<ParameterKey, object>(key, value));
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
    }
}