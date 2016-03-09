// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Luminance effect.
    /// </summary>
    public class LuminanceEffect : ImageEffect
    {
        public static readonly ObjectParameterKey<LuminanceResult> LuminanceResult = ParameterKeys.NewObject<LuminanceResult>();

        private PixelFormat luminanceFormat = PixelFormat.R16_Float;
        private ImageEffectShader luminanceLogEffect;
        private Texture luminance1x1;
        private GaussianBlur blur;

        private ImageMultiScaler multiScaler;
        private ImageReadback<Half> readback;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuminanceEffect" /> class.
        /// </summary>
        public LuminanceEffect()
        {
            LuminanceFormat = PixelFormat.R16_Float;
            DownscaleCount = 6;
            UpscaleCount = 4;
            EnableAverageLuminanceReadback = true;
            readback = new ImageReadback<Half>();
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            LuminanceLogEffect = ToLoadAndUnload(new LuminanceLogEffect());

            // Create 1x1 texture
            luminance1x1 = Texture.New2D(GraphicsDevice, 1, 1, 1, luminanceFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);

            // Use a multiscaler
            multiScaler = ToLoadAndUnload(new ImageMultiScaler());

            // Readback is always going to be done on the 1x1 texture
            readback = ToLoadAndUnload(readback);

            // Blur used before upscaling 
            blur = ToLoadAndUnload(new GaussianBlur());
            blur.Radius = 4;
        }

        /// <summary>
        /// Luminance texture format.
        /// </summary>
        public PixelFormat LuminanceFormat
        {
            get
            {
                return luminanceFormat;
            }

            set
            {
                if (value.IsCompressed() || value.IsPacked() || value.IsTypeless() || value == PixelFormat.None)
                {
                    throw new ArgumentOutOfRangeException("luminanceFormat", "Unsupported format [{0}] (must be not none, compressed, packed or typeless)".ToFormat(luminanceFormat));
                }
                luminanceFormat = value;
            }
        }

        /// <summary>
        /// Luminance log effect.
        /// </summary>
        public ImageEffectShader LuminanceLogEffect
        {
            get
            {
                return luminanceLogEffect;
            }
            set
            {
                luminanceLogEffect = value;
            }
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

        public override void Reset()
        {
            readback.Reset();

            base.Reset();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var input = GetSafeInput(0);
            var output = GetSafeOutput(0);

            // Render the luminance to a power-of-two target, so we preserve energy on downscaling
            var startWidth = Math.Max(1, Math.Min(MathUtil.NextPowerOfTwo(input.Size.Width), MathUtil.NextPowerOfTwo(input.Size.Height)) / 2);
            var startSize = new Size3(startWidth, startWidth, 1);
            var blurTextureSize = startSize.Down2(UpscaleCount);

            Texture outputTextureDown = null;
            if (blurTextureSize.Width != 1 && blurTextureSize.Height != 1)
            {
                outputTextureDown = NewScopedRenderTarget2D(blurTextureSize.Width, blurTextureSize.Height, luminanceFormat, 1);
            }

            var luminanceMap = NewScopedRenderTarget2D(startSize.Width, startSize.Height, luminanceFormat, 1);

            // Calculate the first luminance map
            luminanceLogEffect.SetInput(input);
            luminanceLogEffect.SetOutput(luminanceMap);
            ((RendererBase)luminanceLogEffect).Draw(context);

            // Downscales luminance up to BlurTexture (optional) and 1x1
            multiScaler.SetInput(luminanceMap);
            if (outputTextureDown == null)
            {
                multiScaler.SetOutput(luminance1x1);
            }
            else
            {
                multiScaler.SetOutput(outputTextureDown, luminance1x1);
            }
            multiScaler.Draw(context);

            // If we have an output texture
            if (outputTextureDown != null)
            {
                // Blur x2 the intermediate output texture 
                blur.SetInput(outputTextureDown);
                blur.SetOutput(outputTextureDown);
                ((RendererBase)blur).Draw(context);
                ((RendererBase)blur).Draw(context);

                // Upscale from intermediate to output
                multiScaler.SetInput(outputTextureDown);
                multiScaler.SetOutput(output);
                ((RendererBase)multiScaler).Draw(context);
            }
            else
            {
                // TODO: Workaround to that the output filled with 1x1
                Scaler.SetInput(luminance1x1);
                Scaler.SetOutput(output);
                ((RendererBase)Scaler).Draw(context);
            }

            // Calculate average luminance only if needed
            if (EnableAverageLuminanceReadback)
            {
                readback.SetInput(luminance1x1);
                readback.Draw(context);
                var rawLogValue = readback.Result[0];
                AverageLuminance = (float)Math.Pow(2.0, rawLogValue);

                // In case AvergaeLuminance go crazy because of halp float/infinity precision, some code to save the values here:
                //if (float.IsInfinity(AverageLuminance))
                //{
                //    using (var stream = new FileStream("luminance_input.dds", FileMode.Create, FileAccess.Write))
                //    {
                //        input.Save(stream, ImageFileType.Dds);
                //    }
                //    using (var stream = new FileStream("luminance.dds", FileMode.Create, FileAccess.Write))
                //    {
                //        luminanceMap.Save(stream, ImageFileType.Dds);
                //    }
                //}
            }
        }
    }
}