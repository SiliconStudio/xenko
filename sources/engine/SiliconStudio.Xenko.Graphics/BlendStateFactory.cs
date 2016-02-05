// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Base factory for <see cref="BlendState"/>.
    /// </summary>
    public class BlendStateFactory : ComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlendStateFactory"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal BlendStateFactory(GraphicsDevice device)
        {
            var blendDescription = new BlendStateDescription(Blend.One, Blend.Zero);
            blendDescription.SetDefaults();
            Default = blendDescription;

            Additive = new BlendStateDescription(Blend.SourceAlpha, Blend.One);

            AlphaBlend = new BlendStateDescription(Blend.One, Blend.InverseSourceAlpha);

            NonPremultiplied = new BlendStateDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha);

            Opaque = new BlendStateDescription(Blend.One, Blend.Zero);

            var colorDisabledDescription = new BlendStateDescription();
            colorDisabledDescription.SetDefaults();
            colorDisabledDescription.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.None;
            ColorDisabled = colorDisabledDescription;
        }

        /// <summary>
        /// A built-in state object with settings for default blend, that is no blend at all.
        /// </summary>
        public readonly BlendStateDescription Default;

        /// <summary>
        /// A built-in state object with settings for additive blend, that is adding the destination data to the source data without using alpha.
        /// </summary>
        public readonly BlendStateDescription Additive;

        /// <summary>
        /// A built-in state object with settings for alpha blend, that is blending the source and destination data using alpha.
        /// </summary>
        public readonly BlendStateDescription AlphaBlend;

        /// <summary>
        /// A built-in state object with settings for blending with non-premultipled alpha, that is blending source and destination data using alpha while assuming the color data contains no alpha information.
        /// </summary>
        public readonly BlendStateDescription NonPremultiplied;

        /// <summary>
        /// A built-in state object with settings for opaque blend, that is overwriting the source with the destination data.
        /// </summary>
        public readonly BlendStateDescription Opaque;

        /// <summary>
        /// A built-in state object with settings for no color rendering on target 0, that is only render to depth stencil buffer.
        /// </summary>
        public readonly BlendStateDescription ColorDisabled;
    }
}

