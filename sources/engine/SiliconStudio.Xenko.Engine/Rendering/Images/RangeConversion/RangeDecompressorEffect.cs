// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A RangeDecompressorEffect pass.
    /// </summary>
    [DataContract("RangeDecompressorEffect")]
    public class RangeDecompressorEffect : ImageEffectShader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeDecompressorEffect"/> class.
        /// </summary>
        public RangeDecompressorEffect() : this("RangeDecompressorShaderEffect")
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        /// <param name="decompressorShaderName">Name of the antialias shader.</param>
        /// <exception cref="System.ArgumentNullException">antialiasShaderName</exception>
        public RangeDecompressorEffect(string decompressorShaderName) : base(decompressorShaderName)
        {
            if (decompressorShaderName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(decompressorShaderName));
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

        }
    }
}
