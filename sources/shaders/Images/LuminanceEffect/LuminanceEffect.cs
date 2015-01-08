// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Luminance effect.
    /// </summary>
    public class LuminanceEffect : ImageEffect
    {
        public static readonly ParameterKey<LuminanceResult> LuminanceResult = ParameterKeys.New<LuminanceResult>();

        private readonly PixelFormat luminanceFormat;
        private readonly ImageEffectShader luminanceLogEffect;
        private readonly Texture luminance1x1;
        private readonly GaussianBlur blur;

        private readonly ImageMultiScaler multiScaler;
        private readonly ImageReadback<Half> readback;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuminanceEffect" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="luminanceFormat">The luminance format.</param>
        /// <param name="luminanceLogEffect">The luminance log effect.</param>
        /// <exception cref="System.ArgumentNullException">lunkinanceLogShader</exception>
        public LuminanceEffect(ImageEffectContext context, PixelFormat luminanceFormat = PixelFormat.R16_Float, ImageEffectShader luminanceLogEffect = null) : base(context)
        {
            // Check luminance format
            if (luminanceFormat.IsCompressed() || luminanceFormat.IsPacked() || luminanceFormat.IsTypeless() || luminanceFormat == PixelFormat.None)
            {
                throw new ArgumentOutOfRangeException("luminanceFormat", "Unsupported format [{0}] (must be not none, compressed, packed or typeless)".ToFormat(luminanceFormat));
            }
            this.luminanceFormat = luminanceFormat;
            
            // Use or create a default luminance log effect
            this.luminanceLogEffect = luminanceLogEffect ?? new LuminanceLogEffect(context).DisposeBy(this);

            // Create 1x1 texture
            luminance1x1 = Texture.New2D(GraphicsDevice, 1, 1, 1, luminanceFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);

            // Use a multiscaler
            multiScaler = new ImageMultiScaler(context).DisposeBy(this);

            // Readback is always going to be done on the 1x1 texture
            readback = new ImageReadback<Half>(context).DisposeBy(this);
            readback.SetInput(luminance1x1);

            // Blur used before upscaling 
            blur = new GaussianBlur(context).DisposeBy(this);
            blur.Radius = 4;

            DownscaleCount = 6;
            UpscaleCount = 4;

            EnableAverageLuminanceReadback = true;
        }

        /// <summary>
        /// Gets or sets down scale count used to downscale the input intermediate texture used for local luminance (if no 
        /// output is given). By default 1/64 of the input texture size.
        /// </summary>
        /// <value>Down scale count.</value>
        public int DownscaleCount { get; set; }

        /// <summary>
        /// Gets or sets the upscale count used to upscale the downscaled input local luminance texture. By default x16 of the 
        /// input texture size.
        /// </summary>
        /// <value>The upscale count.</value>
        public int UpscaleCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable calculation of <see cref="AverageLuminance"/> (default is true).
        /// </summary>
        /// <value><c>true</c> if to enable calculation of <see cref="AverageLuminance"/>; otherwise, <c>false</c>.</value>
        public bool EnableAverageLuminanceReadback { get; set; }

        /// <summary>
        /// Gets the average luminance calculated on the GPU. See remarks.
        /// </summary>
        /// <value>The average luminance.</value>
        /// <remarks>
        /// The average luminance is calculated on the GPU and readback with a few frames of delay, depending on the number of 
        /// frames in advance between command scheduling and actual execution on GPU.
        /// </remarks>
        public float AverageLuminance { get; private set; }

        /// <summary>
        /// Gets the average luminance 1x1 texture available after drawing this effect.
        /// </summary>
        /// <value>The average luminance texture.</value>
        public Texture AverageLuminanceTexture
        {
            get
            {
                return luminance1x1;
            }
        }

        protected override void DrawCore()
        {
            var input = GetSafeInput(0);
            var output = GetSafeOutput(0);

            var blurTextureSize = output.Size.Down2(UpscaleCount);
            var outputTextureDown = NewScopedRenderTarget2D(blurTextureSize.Width, blurTextureSize.Height, luminanceFormat, 1);

            var luminanceMap = NewScopedRenderTarget2D(input.ViewWidth, input.ViewHeight, luminanceFormat, 1);

            // Calculate the first luminance map
            luminanceLogEffect.SetInput(input);
            luminanceLogEffect.SetOutput(luminanceMap);
            luminanceLogEffect.Draw();

            // Downscales luminance up to BlurTexture (optional) and 1x1
            multiScaler.SetInput(luminanceMap);
            multiScaler.SetOutput(outputTextureDown, luminance1x1);
            multiScaler.Draw();

            // If we have an output texture
            if (outputTextureDown != null)
            {
                // Blur x2 the intermediate output texture 
                blur.SetInput(outputTextureDown);
                blur.SetOutput(outputTextureDown);
                blur.Draw();
                blur.Draw();

                // Upscale from intermediate to output
                multiScaler.SetInput(outputTextureDown);
                multiScaler.SetOutput(output);
                multiScaler.Draw();
            }

            // Calculate average luminance only if needed
            if (EnableAverageLuminanceReadback)
            {
                readback.Draw();
                var rawLogValue = readback.Result[0];
                AverageLuminance = (float)Math.Pow(2.0, rawLogValue);
            }
        }
    }
}