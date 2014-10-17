// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Generates key-value permutations.
    /// </summary>
    public interface IEffectParameterGenerator
    {
        /// <summary>
        /// Generates key-value permutations.
        /// </summary>
        /// <returns>An enumeration of key-value.</returns>
        IEnumerable<KeyValuePair<ParameterKey, object>> Generate();
    }
}