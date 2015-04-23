// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A default bundle of <see cref="ImageEffect"/>.
    /// </summary>
    [DataContract("PostProcessingEffects")]
    [Display("Post-Processing Effects")]
    public sealed class PostProcessingEffects : ImageEffect, IImageEffectRenderer
    {
        private DepthOfField depthOfField;
        private LuminanceEffect luminanceEffect;
        private BrightFilter brightFilter;
        private Bloom bloom;
        private LightStreak lightStreak;
        private LensFlare lensFlare;
        private ColorTransformGroup colorTransformsGroup;
        private IScreenSpaceAntiAliasingEffect ssaa;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessingEffects" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public PostProcessingEffects(IServiceRegistry services)
            : this(RenderContext.GetShared(services))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessingEffects"/> class.
        /// </summary>
        public PostProcessingEffects()
        {
            depthOfField = new DepthOfField();
            luminanceEffect = new LuminanceEffect();
            brightFilter = new BrightFilter();
            bloom = new Bloom();
            lightStreak = new LightStreak();
            lensFlare = new LensFlare();
            ssaa = new FXAAEffect();
            colorTransformsGroup = new ColorTransformGroup();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessingEffects"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PostProcessingEffects(RenderContext context)
            : this()
        {
            Initialize(context);
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        [DataMember(5)]
        public SceneCameraSlotIndex Camera { get; set; }

        /// <summary>
        /// Gets the depth of field effect.
        /// </summary>
        /// <value>The depth of field.</value>
        [DataMember(10)]
        [Category]
        public DepthOfField DepthOfField
        {
            get
            {
                return depthOfField;
            }
        }

        /// <summary>
        /// Gets the bright pass-filter.
        /// </summary>
        /// <value>The bright filter.</value>
        [DataMember(20)]
        [Category]
        public BrightFilter BrightFilter
        {
            get
            {
                return brightFilter;
            }
        }

        /// <summary>
        /// Gets the bloom effect.
        /// </summary>
        /// <value>The bloom.</value>
        [DataMember(30)]
        [Category]
        public Bloom Bloom
        {
            get
            {
                return bloom;
            }
        }

        /// <summary>
        /// Gets the light streak effect.
        /// </summary>
        /// <value>The light streak.</value>
        [DataMember(40)]
        [Category]
        public LightStreak LightStreak
        {
            get
            {
                return lightStreak;
            }
        }

        /// <summary>
        /// Gets the lens flare effect.
        /// </summary>
        /// <value>The lens flare.</value>
        [DataMember(50)]
        [Category]
        public LensFlare LensFlare
        {
            get
            {
                return lensFlare;
            }
        }

        /// <summary>
        /// Gets the final color transforms.
        /// </summary>
        /// <value>The color transforms.</value>
        [DataMember(60)]
        [Category]
        public ColorTransformGroup ColorTransforms
        {
            get
            {
                return colorTransformsGroup;
            }
        }

        /// <summary>
        /// Gets the antialiasing effect.
        /// </summary>
        /// <value>The antialiasing.</value>
        [DataMember(70)]
        [Category]
        public IScreenSpaceAntiAliasingEffect Antialiasing
        {
            get
            {
                return ssaa;
            }

            set
            {
                // TODO: Unload previous anti-aliasing before replacing it
                ssaa = value;
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            depthOfField = ToLoadAndUnload(depthOfField);
            luminanceEffect = ToLoadAndUnload(luminanceEffect);
            brightFilter = ToLoadAndUnload(brightFilter);
            bloom = ToLoadAndUnload(bloom);
            lightStreak = ToLoadAndUnload(lightStreak);
            lensFlare = ToLoadAndUnload(lensFlare);
            ssaa = ToLoadAndUnload(ssaa);
            colorTransformsGroup = ToLoadAndUnload(colorTransformsGroup);
        }

        protected override void DrawCore(RenderContext context)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null || output == null)
            {
                return;
            }

            // Gets the current camera state 
            var camera = context.GetCameraFromSlot(Camera);
            if (camera != null)
            {
                // Update the parameters for this post effect
                CameraComponentRenderer.UpdateParameters(context, camera);
            }

            if (!Enabled)
            {
                if (input != output)
                {
                    Scaler.SetInput(input);
                    Scaler.SetOutput(output);
                    Scaler.Draw(context);
                }
                return;
            }

            // If input == output, than copy the input to a temporary texture
            if (input == output)
            {
                var newInput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                GraphicsDevice.Copy(input, newInput);
                input = newInput;
            }
            
            var currentInput = input;

            if (depthOfField.Enabled && InputCount > 1 && GetInput(1) != null && GetInput(1).IsDepthStencil)
            {
                // DoF
                var dofOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                var inputDepthTexture = GetInput(1); // Depth
                depthOfField.SetColorDepthInput(input, inputDepthTexture);
                depthOfField.SetOutput(dofOutput);
                depthOfField.Draw(context);
                currentInput = dofOutput;
            }

            // Luminance pass (only if tone mapping is enabled)
            // TODO: This is not super pluggable to have this kind of dependencies. Check how to improve this
            if (colorTransformsGroup.Enabled && colorTransformsGroup.Transforms.IsEnabled<ToneMap>())
            {
                const int LocalLuminanceDownScale = 3;
                var lumSize = currentInput.Size.Down2(LocalLuminanceDownScale);
                var luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                luminanceEffect.SetInput(currentInput);
                luminanceEffect.SetOutput(luminanceTexture);
                luminanceEffect.Draw(context);

                // Set this parameter that will be used by the tone mapping
                colorTransformsGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
            }

            // Bright filter pass
            Texture brightTexture = null;
            if (bloom.Enabled || lightStreak.Enabled || lensFlare.Enabled)
            {
                brightTexture = NewScopedRenderTarget2D(currentInput.Width, currentInput.Height, currentInput.Format, 1);

                brightFilter.SetInput(currentInput);
                brightFilter.SetOutput(brightTexture);
                brightFilter.Draw(context);
            }

            // Bloom pass
            if (bloom.Enabled)
            {
                bloom.SetInput(brightTexture);
                bloom.SetOutput(currentInput);
                bloom.Draw(context);
            }

            // Light streak pass
            if (lightStreak.Enabled)
            {
                lightStreak.SetInput(brightTexture);
                lightStreak.SetOutput(currentInput);
                lightStreak.Draw(context);
            }

            // Lens flare pass
            if (lensFlare.Enabled)
            {
                lensFlare.SetInput(brightTexture);
                lensFlare.SetOutput(currentInput);
                lensFlare.Draw(context);
            }

            var outputForLastEffectBeforeAntiAliasing = output;

            if (ssaa != null && ssaa.Enabled)
            {
                outputForLastEffectBeforeAntiAliasing = NewScopedRenderTarget2D(output.Width, output.Height, output.Format);
            }

            // Color transform group pass (tonemap, color grading, gamma correction)
            var lastEffect = colorTransformsGroup.Enabled ? (ImageEffect)colorTransformsGroup: Scaler;
            lastEffect.SetInput(currentInput);
            lastEffect.SetOutput(outputForLastEffectBeforeAntiAliasing);
            lastEffect.Draw(context);

            if (ssaa != null && ssaa.Enabled)
            {
                ssaa.SetInput(outputForLastEffectBeforeAntiAliasing);
                ssaa.SetOutput(output);
                ssaa.Draw(context);
            }
        }
    }
}