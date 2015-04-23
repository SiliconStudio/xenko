// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A FXAA anti-aliasing pass.
    /// </summary>
    [DataContract("FXAAEffect")]
    public class FXAAEffect : ImageEffectShader, IScreenSpaceAntiAliasingEffect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        public FXAAEffect() : this("FXAAShader")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        /// <param name="antialiasShaderName">Name of the antialias shader.</param>
        /// <exception cref="System.ArgumentNullException">antialiasShaderName</exception>
        public FXAAEffect(string antialiasShaderName) : base(antialiasShaderName)
        {
            if (antialiasShaderName == null) throw new ArgumentNullException("antialiasShaderName");
        }
    }
}