// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A FXAA anti-aliasing pass.
    /// </summary>
    [DataContract("FXAAEffect")]
    public class FXAAEffect : ImageEffectShader, IScreenSpaceAntiAliasingEffect
    {
        private const int DefaultQuality = 15;
        internal static readonly PermutationParameterKey<int> GreenAsLumaKey = ParameterKeys.NewPermutation(0);
        internal static readonly PermutationParameterKey<int> QualityKey = ParameterKeys.NewPermutation(15);

        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        public FXAAEffect() : this("FXAAShaderEffect")
        {
            Quality = DefaultQuality;
            InputLuminanceInAlpha = true;
        }

        /// <summary>
        /// Animates the film grain.
        /// </summary>
        /// <userdoc>The quality of the anti-alising filter.</userdoc>
        [DataMember(10)]
        [DefaultValue(DefaultQuality)]
        [DataMemberRange(10, 39, 1, 5, 0)]
        public int Quality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the luminance will be retrieved from the alpha channel of the input color. Otherwise, the green component of the input color is used as a luminance.
        /// </summary>
        /// <value><c>true</c> the luminance will be retrieved from the alpha channel of the input color. Otherwise, the green component of the input color is used as a luminance.</value>
        /// <userdoc>The luminance will be retrieved from the alpha channel of the input color. Otherwise, the green component of the input color is used as an approximation to the luminance.</userdoc>
        [DataMember(20)]
        [DefaultValue(true)]
        [Display("Input luminance from alpha?")]
        public bool InputLuminanceInAlpha { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FXAAEffect"/> class.
        /// </summary>
        /// <param name="antialiasShaderName">Name of the antialias shader.</param>
        /// <exception cref="System.ArgumentNullException">antialiasShaderName</exception>
        public FXAAEffect(string antialiasShaderName) : base(antialiasShaderName)
        {
            if (antialiasShaderName == null) throw new ArgumentNullException("antialiasShaderName");
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);
            Parameters.Set(GreenAsLumaKey, InputLuminanceInAlpha ? 0 : 1);
            Parameters.Set(QualityKey, Quality);
        }
    }
}