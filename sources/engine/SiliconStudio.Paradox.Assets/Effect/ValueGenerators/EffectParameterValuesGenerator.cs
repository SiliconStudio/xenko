// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect.ValueGenerators
{
    /// <summary>
    /// A generator that contains only values.
    /// </summary>
    [DataContract]
    public class EffectParameterValuesGenerator<T> : List<T>, IEffectParameterValueGenerator
    {
        IEnumerable<object> IEffectParameterValueGenerator.GenerateValues(ParameterKey key)
        {
            return this.Select(value => key.ConvertValue(value));
        }

        void IEffectParameterValueGenerator.AddValue(ParameterKey key, object value)
        {
            this.Add((T)key.ConvertValue(value));
        }
    }
}