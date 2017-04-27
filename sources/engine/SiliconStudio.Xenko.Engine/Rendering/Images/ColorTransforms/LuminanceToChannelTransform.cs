// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using System.Globalization;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A color transform to output the luminance to a specific channel.
    /// </summary>
    [DataContract("LuminanceToChannelTransform")]
    public class LuminanceToChannelTransform : ColorTransform
    {
        private ColorChannel colorChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuminanceToChannelTransform"/> class.
        /// </summary>
        public LuminanceToChannelTransform()
            : this("LuminanceToChannelShader")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuminanceToChannelTransform" /> class.
        /// </summary>
        /// <param name="colorTransformShader">Name of the shader.</param>
        public LuminanceToChannelTransform(string colorTransformShader) : base(colorTransformShader)
        {
            ColorChannel = ColorChannel.A;
        }

        /// <summary>
        /// Gets or sets the color channel to output the luminance. Default is alpha.
        /// </summary>
        /// <value>The color channel.</value>
        [DefaultValue(ColorChannel.A)]
        public ColorChannel ColorChannel
        {
            get
            {
                return colorChannel;
            }
            set
            {
                colorChannel = value;
                GenericArguments = new object[] { value.ToString().ToLowerInvariant() };
            }
        }
    }
}
