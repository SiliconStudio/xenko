// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A default bundle of <see cref="ImageEffect"/>.
    /// </summary>
    public class ImageEffectBundle : ImageEffect
    {
        private readonly LuminanceEffect luminanceEffect;
        private readonly BrightFilter brightFilter;
        private readonly Bloom bloom;
        private readonly ColorTransformGroup colorTransformGroup;
        private readonly ToneMap toneMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectBundle"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public ImageEffectBundle(IServiceRegistry services)
            : this(ImageEffectContext.GetShared(services))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectBundle"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ImageEffectBundle(ImageEffectContext context)
            : base(context)
        {
            luminanceEffect = new LuminanceEffect(Context);
            brightFilter = new BrightFilter(Context);
            bloom = new Bloom(Context);
            colorTransformGroup = new ColorTransformGroup(Context);
            toneMap = new ToneMap();
            colorTransformGroup.Transforms.Add(toneMap);
        }

        public BrightFilter BrightFilter
        {
            get
            {
                return brightFilter;
            }
        }

        public Bloom Bloom
        {
            get
            {
                return bloom;
            }
        }

        public ColorTransformGroup ColorTransform
        {
            get
            {
                return colorTransformGroup;
            }
        }

        protected override void DrawCore()
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null || input == output)
            {
                return;
            }

            // TODO: Add DOF/MotionBlur pass

            // Luminance pass (only if tone mapping is enabled)
            if (toneMap.Enabled)
            {
                const int LocalLuminanceDownScale = 3;
                var lumSize = input.Size.Down2(LocalLuminanceDownScale);
                var luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                luminanceEffect.SetInput(input);
                luminanceEffect.SetOutput(luminanceTexture);
                luminanceEffect.Draw();

                // Set this parameter that will be used by the tone mapping
                colorTransformGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
            }

            // Bloom pass
            // TODO: Add Glare pass
            if (bloom.Enabled)
            {
                var brightTexture = NewScopedRenderTarget2D(input.Width, input.Height, input.Format, 1);

                brightFilter.SetInput(input);
                brightFilter.SetOutput(brightTexture);
                brightFilter.Draw();

                bloom.SetInput(brightTexture);
                bloom.SetOutput(input);
                bloom.Draw();
            }

            // Color transform group pass (tonemap, color grading, gamma correction)
            var lastEffect = colorTransformGroup.Enabled ? (ImageEffect)colorTransformGroup: Scaler;
            lastEffect.SetInput(input);
            lastEffect.SetOutput(output);
            lastEffect.Draw();

            // TODO: Add anti aliasing pass
        }

    }
}