// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Defines keys associated with mesh used for compiling assets.
    /// </summary>
    public static class EffectKeys
    {
        /// <summary>
        /// Name of the effect to compile (either a pdxfx or standalone pdxsl)
        /// </summary>
        public static readonly ParameterKey<string> Name = ParameterKeys.New<string>();
    }
}