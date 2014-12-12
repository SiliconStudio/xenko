// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageEffectGroup : ImageEffect
    {
        private readonly LuminanceEffect luminanceEffect;

        private readonly BrightFilter brightFilter;

        private readonly Bloom bloom;

        private readonly ColorTransformGroup colorTransformGroup;

        private readonly ToneMap toneMap;

        public ImageEffectGroup(IServiceRegistry services)
            : this(ImageEffectContext.GetShared(services))
        {
        }

        public ImageEffectGroup(ImageEffectContext context)
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

            // Luminance pass (only if tone mapping is enabled)
            if (toneMap.Enabled)
            {
                const int LocalLuminanceDownScale = 6;
                var lumSize = input.Size.Down2(LocalLuminanceDownScale);
                var luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                luminanceEffect.SetInput(input);
                luminanceEffect.SetOutput(luminanceTexture);
                luminanceEffect.Draw();

                // Set this parameter that will be used by the tone mapping
                colorTransformGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
            }

            // TODO: Add DOF/MotionBlur pass

            // Bloom pass
            // TODO: Add Glare pass
            if (bloom.Enabled)
            {
                const int LocalBrightDownScale = 4;
                var brightSize = input.Size.Down2(LocalBrightDownScale);
                var brightTexture = NewScopedRenderTarget2D(brightSize.Width, brightSize.Height, input.Format, 1);

                brightFilter.SetInput(input);
                brightFilter.SetOutput(brightTexture);
                brightFilter.Draw();

                bloom.SetInput(brightTexture);
                bloom.SetOutput(input);
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