// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
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
        private readonly FXAAEffect fxaa;
        private readonly DepthOfField depthOfField;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectBundle"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public ImageEffectBundle(IServiceRegistry services)
            : this(DrawEffectContext.GetShared(services))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectBundle"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ImageEffectBundle(DrawEffectContext context)
            : base(context)
        {
            luminanceEffect = new LuminanceEffect(Context);
            brightFilter = new BrightFilter(Context);
            bloom = new Bloom(Context);
            colorTransformGroup = new ColorTransformGroup(Context);
            fxaa = new FXAAEffect(Context);
            toneMap = new ToneMap();
            colorTransformGroup.Transforms.Add(toneMap);

            // TODO: Add presets instead
            depthOfField = new DepthOfField(context)
            {
                Technique = BokehTechnique.HexagonalTripleRhombi,
                DOFAreas = new Vector4(0.5f, 6f, 50f, 200f),
                LevelCoCValues = new float[] { 0.25f, 0.5f, 1.0f },
                LevelDownscaleFactors = new int[] { 1, 1, 1 },
                MaxBokehSize = 10f / 1280f // Make parameter ratio relative to input texture size?
            };
            // Example of DoF configuration
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

        public ToneMap ToneMap
        {
            get
            {
                return toneMap;
            }
        }

        public FXAAEffect Antialiasing
        {
            get
            {
                return fxaa;
            }
        }

        public ColorTransformGroup ColorTransform
        {
            get
            {
                return colorTransformGroup;
            }
        }

        public DepthOfField DepthOfField
        {
            get
            {
                return depthOfField;
            }
        }

        protected override void DrawCore(ParameterCollection contextParameters)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null || input == output)
            {
                return;
            }
            
            var currentInput = input;

            if (depthOfField.Enabled && InputCount > 1 && GetInput(1) != null && GetInput(1).IsDepthStencil)
            {
                // DoF
                var dofOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                var inputDepthTexture = GetInput(1); // Depth
                depthOfField.SetColorDepthInput(input, inputDepthTexture);
                depthOfField.SetOutput(dofOutput);
                depthOfField.Draw(contextParameters);
                currentInput = dofOutput;
            }

            // Luminance pass (only if tone mapping is enabled)
            if (toneMap.Enabled)
            {
                const int LocalLuminanceDownScale = 3;
                var lumSize = currentInput.Size.Down2(LocalLuminanceDownScale);
                var luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                luminanceEffect.SetInput(currentInput);
                luminanceEffect.SetOutput(luminanceTexture);
                luminanceEffect.Draw(contextParameters);

                // Set this parameter that will be used by the tone mapping
                colorTransformGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
            }

            // Bloom pass
            // TODO: Add Glare pass
            if (bloom.Enabled)
            {
                var brightTexture = NewScopedRenderTarget2D(currentInput.Width, currentInput.Height, currentInput.Format, 1);

                brightFilter.SetInput(currentInput);
                brightFilter.SetOutput(brightTexture);
                brightFilter.Draw(contextParameters);

                bloom.SetInput(brightTexture);
                bloom.SetOutput(currentInput);
                bloom.Draw(contextParameters);
            }

            var outputForLastEffectBeforeAntiAliasing = output;

            if (fxaa.Enabled)
            {
                outputForLastEffectBeforeAntiAliasing = NewScopedRenderTarget2D(output.Width, output.Height, output.Format);
            }

            // Color transform group pass (tonemap, color grading, gamma correction)
            var lastEffect = colorTransformGroup.Enabled ? (ImageEffect)colorTransformGroup: Scaler;
            lastEffect.SetInput(currentInput);
            lastEffect.SetOutput(outputForLastEffectBeforeAntiAliasing);
            lastEffect.Draw(contextParameters);

            if (fxaa.Enabled)
            {
                fxaa.SetInput(outputForLastEffectBeforeAntiAliasing);
                fxaa.SetOutput(output);
                fxaa.Draw(contextParameters);
            }

            // TODO: Add anti aliasing pass
        }

    }
}