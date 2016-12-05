// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A RangeCompressorEffect pass.
    /// </summary>
    [DataContract("RangeCompressorEffect")]
    public class RangeCompressorEffect : ImageEffectShader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeCompressorEffect"/> class.
        /// </summary>
        public RangeCompressorEffect() : this("RangeCompressorShaderEffect")
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        /// <param name="compressorShaderName">Name of the antialias shader.</param>
        /// <exception cref="System.ArgumentNullException">antialiasShaderName</exception>
        public RangeCompressorEffect(string compressorShaderName) : base(compressorShaderName)
        {
            if (compressorShaderName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(compressorShaderName));
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

        }
    }
}
