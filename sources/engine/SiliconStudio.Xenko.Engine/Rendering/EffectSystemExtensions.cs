// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Extensions for <see cref="EffectSystem"/>
    /// </summary>
    public static class EffectSystemExtensions
    {
        /// <summary>
        /// Creates an effect.
        /// </summary>
        /// <param name="effectSystem">The effect system.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <returns>A new instance of an effect.</returns>
        public static TaskOrResult<Effect> LoadEffect(this EffectSystem effectSystem, string effectName)
        {
            var compilerParameters = new CompilerParameters();
            return effectSystem.LoadEffect(effectName, compilerParameters);
        }
    }
}