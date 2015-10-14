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
            Default = BlendState.New(device, blendDescription).DisposeBy(device);
            Default.Name = "Default";

            Additive = BlendState.New(device, new BlendStateDescription(Blend.SourceAlpha, Blend.One)).DisposeBy(device);
            Additive.Name = "Additive";

            AlphaBlend = BlendState.New(device, new BlendStateDescription(Blend.One, Blend.InverseSourceAlpha)).DisposeBy(device);
            AlphaBlend.Name = "AlphaBlend";

            NonPremultiplied = BlendState.New(device, new BlendStateDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha)).DisposeBy(device);
            NonPremultiplied.Name = "NonPremultiplied";

            Opaque = BlendState.New(device, new BlendStateDescription(Blend.One, Blend.Zero)).DisposeBy(device);
            Opaque.Name = "Opaque";

            var colorDisabledDescription = new BlendStateDescription();
            colorDisabledDescription.SetDefaults();
            colorDisabledDescription.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.None;
            ColorDisabled = BlendState.New(device, colorDisabledDescription).DisposeBy(device);
            ColorDisabled.Name = "ColorDisabled";
        }

        /// <summary>
        /// A built-in state object with settings for default blend, that is no blend at all.
        /// </summary>
        public readonly BlendState Default;

        /// <summary>
        /// A built-in state object with settings for additive blend, that is adding the destination data to the source data without using alpha.
        /// </summary>
        public readonly BlendState Additive;

        /// <summary>
        /// A built-in state object with settings for alpha blend, that is blending the source and destination data using alpha.
        /// </summary>
        public readonly BlendState AlphaBlend;

        /// <summary>
        /// A built-in state object with settings for blending with non-premultipled alpha, that is blending source and destination data using alpha while assuming the color data contains no alpha information.
        /// </summary>
        public readonly BlendState NonPremultiplied;

        /// <summary>
        /// A built-in state object with settings for opaque blend, that is overwriting the source with the destination data.
        /// </summary>
        public readonly BlendState Opaque;

        /// <summary>
        /// A built-in state object with settings for no color rendering on target 0, that is only render to depth stencil buffer.
        /// </summary>
        public readonly BlendState ColorDisabled;
    }
}

