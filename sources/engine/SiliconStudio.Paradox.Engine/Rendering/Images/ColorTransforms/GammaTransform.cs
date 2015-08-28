// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A Gamma <see cref="ColorTransformBase"/>.
    /// </summary>
    [DataContract("GammaTransform")]
    public class GammaTransform : ColorTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GammaTransform"/> class.
        /// </summary>
        public GammaTransform() : this("GammaTransformShader")
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GammaTransform" /> class.
        /// </summary>
        /// <param name="colorTransformShader">Name of the shader.</param>
        public GammaTransform(string colorTransformShader) : base(colorTransformShader)
        {
            Automatic = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GammaTransform"/> is automatically enabled/disabled depending on the global <see cref="GraphicsDevice.ColorSpace"/>.
        /// </summary>
        /// <value><c>true</c> this <see cref="GammaTransform"/> is automatically enabled/disabled depending on the global <see cref="GraphicsDevice.ColorSpace"/>; otherwise, <c>false</c>.</value>
        /// <userdoc>The Linear to Gamma transformation is automatically performed based on the global ColorSpace stored in the GameSettings</userdoc>
        [DataMember(10)]
        [DefaultValue(true)]
        public bool Automatic { get; set; }

        /// <summary>
        /// Gets or sets the gamma value.
        /// </summary>
        /// <value>The value.</value>
        /// <userdoc>The value of the gamma transformation.</userdoc>
        [DataMember(20)]
        [DefaultValue(2.2333333f)]
        public float Value
        {
            get
            {
                return Parameters.Get(GammaTransformShaderKeys.Gamma);
            }
            set
            {
                Parameters.Set(GammaTransformShaderKeys.Gamma, value);
            }
        }

        public override void UpdateParameters(ColorTransformContext context)
        {
            // Automatically enable/disable GammeTransform if the current color space is not gamma
            bool isEnabled = context.RenderContext.GraphicsDevice.ColorSpace == ColorSpace.Linear;
            if (Automatic && Enabled != isEnabled)
            {
                Enabled = isEnabled;
            }
        }
    }
}