// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Generates values for a specific key.
    /// </summary>
    public interface IEffectParameterValueGenerator
    {
        /// <summary>
        /// Generates the values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An enumeration of values for the specified key.</returns>
        IEnumerable<object> GenerateValues(ParameterKey key);

        /// <summary>
        /// Adds a new value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddValue(ParameterKey key, object value);
    }
}