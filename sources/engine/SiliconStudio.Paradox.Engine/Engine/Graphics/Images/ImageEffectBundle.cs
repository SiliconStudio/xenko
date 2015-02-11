// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A default bundle of <see cref="ImageEffect"/>.
    /// </summary>
    [DataContract("ImageEffectBundle")]
    public sealed class ImageEffectBundle : ImageEffect
    {
        private readonly LuminanceEffect luminanceEffect;
        private readonly BrightFilter brightFilter;
        private readonly Bloom bloom;
        private readonly ColorTransformGroup colorTransformsGroup;
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
        public ImageEffectBundle()
        {
            depthOfField        = ToDispose(new DepthOfField()); 
            luminanceEffect     = ToDispose(new LuminanceEffect());
            brightFilter        = ToDispose(new BrightFilter());
            bloom               = ToDispose(new Bloom());
            fxaa                = ToDispose(new FXAAEffect());
            colorTransformsGroup = ToDispose(new ColorTransformGroup());
            toneMap = new ToneMap();
            colorTransformsGroup.Transforms.Add(toneMap);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectBundle"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ImageEffectBundle(DrawEffectContext context)
            : this()
        {
            Initialize(context);
        }

        [DataMember(10)]
        [Category]
        public DepthOfField DepthOfField
        {
            get
            {
                return depthOfField;
            }
        }

        [DataMember(20)]
        [Category]
        public BrightFilter BrightFilter
        {
            get
            {
                return brightFilter;
            }
        }

        [DataMember(30)]
        [Category]
        public Bloom Bloom
        {
            get
            {
                return bloom;
            }
        }

        [DataMemberIgnore]
        public ToneMap ToneMap
        {
            get
            {
                return toneMap;  // ToneMap is already serialized by ColorTransforms
            }
        }

        [DataMember(40)]
        [Category]
        public ColorTransformGroup ColorTransforms
        {
            get
            {
                return colorTransformsGroup;
            }
        }


        [DataMember(50)]
        [Category]
        public FXAAEffect Antialiasing
        {
            get
            {
                return fxaa; // TOOD: Allow to change the anti-aliasing technique
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
                colorTransformsGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
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
            var lastEffect = colorTransformsGroup.Enabled ? (ImageEffect)colorTransformsGroup: Scaler;
            lastEffect.SetInput(currentInput);
            lastEffect.SetOutput(outputForLastEffectBeforeAntiAliasing);
            lastEffect.Draw(contextParameters);

            if (fxaa.Enabled)
            {
                fxaa.SetInput(outputForLastEffectBeforeAntiAliasing);
                fxaa.SetOutput(output);
                fxaa.Draw(contextParameters);
            }
        }

    }
}