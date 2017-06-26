// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A default bundle of <see cref="ImageEffect"/>.
    /// </summary>
    [DataContract("PostProcessingEffects")]
    [Display("Post-Processing Effects")]
    public sealed class PostProcessingEffects : ImageEffect, IImageEffectRenderer, IPostProcessingEffects
    {
        private AmbientOcclusion ambientOcclusion;
        private LocalReflections localReflections;
        private DepthOfField depthOfField;
        private LuminanceEffect luminanceEffect;
        private ColorTransformGroup colorTransformsGroup;

        private ImageEffectShader rangeCompress;
        private ImageEffectShader rangeDecompress;

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
            AmbientOcclusion = new AmbientOcclusion();
            LocalReflections = new LocalReflections();
            DepthOfField = new DepthOfField();
            luminanceEffect = new LuminanceEffect();
            BrightFilter = new BrightFilter();
            Bloom = new Bloom();
            LightStreak = new LightStreak();
            LensFlare = new LensFlare();
            Antialiasing = new FXAAEffect();
            rangeCompress = new ImageEffectShader("RangeCompressorShader");
            rangeDecompress = new ImageEffectShader("RangeDecompressorShader");
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

        /// <inheritdoc/>
        [DataMember(-100), Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the ambient occlusion effect.
        /// </summary>
        /// <userdoc>
        /// The ambient occlusion post-effect allows you to simulate occlusion for opaque objects which are close to or occluded by other opaque objects.
        /// </userdoc>
        [DataMember(8)]
        [Category]
        public AmbientOcclusion AmbientOcclusion { get; private set; }

        /// <summary>
        /// Gets the local reflections effect.
        /// </summary>
        /// <value>The local reflection technique.</value>
        [DataMember(9)]
        [Category]
        public LocalReflections LocalReflections { get; private set; }

        /// <summary>
        /// Gets the depth of field effect.
        /// </summary>
        /// <value>The depth of field.</value>
        /// <userdoc>The depth of field post-effect allows you to accentuate some regions of your image by blurring object in foreground or background.</userdoc>
        [DataMember(10)]
        [Category]
        public DepthOfField DepthOfField { get; private set; }

        /// <summary>
        /// Gets the bright pass-filter.
        /// </summary>
        /// <value>The bright filter.</value>
        /// <userdoc>The parameters for the bright filter. The bright filter is not an effect by itself. 
        /// It just extracts the brightest areas of the image and gives it to other effect that need it (eg. bloom, light streaks, lens-flares).</userdoc>
        [DataMember(20)]
        [Category]
        public BrightFilter BrightFilter { get; private set; }

        /// <summary>
        /// Gets the bloom effect.
        /// </summary>
        /// <value>The bloom.</value>
        /// <userdoc>Produces a bleeding effect of bright areas onto their surrounding.</userdoc>
        [DataMember(30)]
        [Category]
        public Bloom Bloom { get; private set; }

        /// <summary>
        /// Gets the light streak effect.
        /// </summary>
        /// <value>The light streak.</value>
        /// <userdoc>Produces a bleeding effect of the brightest points of the image along streaks.</userdoc>
        [DataMember(40)]
        [Category]
        public LightStreak LightStreak { get; private set; }

        /// <summary>
        /// Gets the lens flare effect.
        /// </summary>
        /// <value>The lens flare.</value>
        /// <userdoc>Simulates the artifacts produced by the internal reflection or scattering of the light within camera lens.</userdoc>
        [DataMember(50)]
        [Category]
        public LensFlare LensFlare { get; private set; }

        /// <summary>
        /// Gets the final color transforms.
        /// </summary>
        /// <value>The color transforms.</value>
        /// <userdoc>Performs a transformation onto the image colors.</userdoc>
        [DataMember(70)]
        [Category]
        public ColorTransformGroup ColorTransforms => colorTransformsGroup;

        /// <summary>
        /// Gets the antialiasing effect.
        /// </summary>
        /// <value>The antialiasing.</value>
        /// <userdoc>Performs anti-aliasing filtering on the image. This smoothes the jagged edges of models.</userdoc>
        [DataMember(80)]
        [Display("Type", "Antialiasing")]
        public IScreenSpaceAntiAliasingEffect Antialiasing { get; set; } // TODO: Unload previous anti aliasing

        /// <summary>
        /// Disables all post processing effects.
        /// </summary>
        public void DisableAll()
        {
            AmbientOcclusion.Enabled = false;
            LocalReflections.Enabled = false;
            DepthOfField.Enabled = false;
            Bloom.Enabled = false;
            LightStreak.Enabled = false;
            LensFlare.Enabled = false;
            Antialiasing.Enabled = false;
            rangeCompress.Enabled = false;
            rangeDecompress.Enabled = false;
            colorTransformsGroup.Enabled = false;
        }

        public override void Reset()
        {
            // TODO: Check how to reset other effects too
            // Reset the luminance effect
            luminanceEffect.Reset();

            base.Reset();
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            AmbientOcclusion = ToLoadAndUnload(AmbientOcclusion);
            LocalReflections = ToLoadAndUnload(LocalReflections);
            DepthOfField = ToLoadAndUnload(DepthOfField);
            luminanceEffect = ToLoadAndUnload(luminanceEffect);
            BrightFilter = ToLoadAndUnload(BrightFilter);
            Bloom = ToLoadAndUnload(Bloom);
            LightStreak = ToLoadAndUnload(LightStreak);
            LensFlare = ToLoadAndUnload(LensFlare);
            //this can be null if no SSAA is selected in the editor
            if(Antialiasing != null) Antialiasing = ToLoadAndUnload(Antialiasing);

            rangeCompress = ToLoadAndUnload(rangeCompress);
            rangeDecompress = ToLoadAndUnload(rangeDecompress);

            colorTransformsGroup = ToLoadAndUnload(colorTransformsGroup);
        }

        public void Collect(RenderContext context)
        {
        }

        public void Draw(RenderDrawContext drawContext, RenderOutputValidator outputValidator, Texture[] inputs, Texture inputDepthStencil, Texture outputTarget)
        {
            var colorIndex = outputValidator.Find<ColorTargetSemantic>();
            if (colorIndex < 0)
                return;
            
            SetInput(0, inputs[colorIndex]);
            SetInput(1, inputDepthStencil);

            var normalsIndex = outputValidator.Find<NormalTargetSemantic>();
            if (normalsIndex >= 0)
            {
                SetInput(2, inputs[normalsIndex]);
            }

            var specularRoughnessIndex = outputValidator.Find<SpecularColorRoughnessTargetSemantic>();
            if (specularRoughnessIndex >= 0)
            {
                SetInput(3, inputs[specularRoughnessIndex]);
            }

            var reflectionIndex0 = outputValidator.Find<OctahedronNormalSpecularColorTargetSemantic>();
            var reflectionIndex1 = outputValidator.Find<EnvironmentLightRoughnessTargetSemantic>();
            if (reflectionIndex0 >= 0 && reflectionIndex1 >= 0)
            {
                SetInput(4, inputs[reflectionIndex0]);
                SetInput(5, inputs[reflectionIndex1]);
            }
            
            SetOutput(outputTarget);
            Draw(drawContext);
        }

        public bool RequiresVelocityBuffer => false;

        public bool RequiresNormalBuffer => LocalReflections.Enabled;

        public bool RequiresSpecularRoughnessBuffer => LocalReflections.Enabled;

        protected override void DrawCore(RenderDrawContext context)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null || output == null)
            {
                return;
            }

            var inputDepthTexture = GetInput(1); // Depth

            // Update the parameters for this post effect
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
                context.CommandList.Copy(input, newInput);
                input = newInput;
            }
            
            var currentInput = input;

            var fxaa = Antialiasing as FXAAEffect;
            bool aaFirst = Bloom != null && Bloom.StableConvolution;
            bool needAA = fxaa != null && fxaa.Enabled;

            // do AA here, first. (hybrid method from Karis2013)
            if (aaFirst && needAA)
            {
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.HybridFxaa))
                {
                    // explanation:
                    // The Karis method (Unreal Engine 4.1x), uses a hybrid pipeline to execute AA.
                    // The AA is usually done at the end of the pipeline, but we don't benefit from
                    // AA for the posteffects, which is a shame.
                    // The Karis method, executes AA at the beginning, but for AA to be correct, it must work post tonemapping,
                    // and even more in fact, in gamma space too. Plus, it waits for the alpha=luma to be a "perceptive luma" so also gamma space.
                    // in our case, working in gamma space created monstruous outlining artefacts around eggageratedely strong constrasted objects (way in hdr range).
                    // so AA works in linear space, but still with gamma luma, as a light tradeoff to supress artefacts.

                    // create a 16 bits target for FXAA:
                    var aaSurface = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);

                    // render range compression & perceptual luma to alpha channel:
                    rangeCompress.SetInput(currentInput);
                    rangeCompress.SetOutput(aaSurface);
                    rangeCompress.Draw(context);

                    // do AA:
                    fxaa.InputLuminanceInAlpha = true;
                    Antialiasing.SetInput(aaSurface);
                    Antialiasing.SetOutput(currentInput);
                    Antialiasing.Draw(context);

                    // reverse tone LDR to HDR:
                    rangeDecompress.SetInput(currentInput);
                    rangeDecompress.SetOutput(aaSurface);
                    rangeDecompress.Draw(context);
                    currentInput = aaSurface;
                }
            }

            if (AmbientOcclusion.Enabled && inputDepthTexture != null)
            {
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.AmbientOcclusion))
                {
                    // Ambient Occlusion
                    var aoOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                    AmbientOcclusion.SetColorDepthInput(currentInput, inputDepthTexture);
                    AmbientOcclusion.SetOutput(aoOutput);
                    AmbientOcclusion.Draw(context);
                    currentInput = aoOutput;
                }
            }

            if (LocalReflections.Enabled && inputDepthTexture != null)
            {
                var normalsBuffer = GetInput(2);
                var specularRoughnessBuffer = GetInput(3);

                if (normalsBuffer != null && specularRoughnessBuffer != null)
                {
                    // Local reflections
                    var rlrOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                    LocalReflections.SetInputSurfaces(currentInput, inputDepthTexture, normalsBuffer, specularRoughnessBuffer);
                    LocalReflections.SetOutput(rlrOutput);
                    LocalReflections.Draw(context);
                    currentInput = rlrOutput;
                }
            }

            if (DepthOfField.Enabled && inputDepthTexture != null)
            {
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.DepthOfField))
                {
                    // DoF
                    var dofOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                    DepthOfField.SetColorDepthInput(currentInput, inputDepthTexture);
                    DepthOfField.SetOutput(dofOutput);
                    DepthOfField.Draw(context);
                    currentInput = dofOutput;
                }
            }

            // Luminance pass (only if tone mapping is enabled)
            // TODO: This is not super pluggable to have this kind of dependencies. Check how to improve this
            var toneMap = colorTransformsGroup.Transforms.Get<ToneMap>();
            if (colorTransformsGroup.Enabled && toneMap != null && toneMap.Enabled)
            {
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.AverageLumiance))
                {
                    Texture luminanceTexture = null;
                    if (toneMap.UseLocalLuminance)
                    {
                        const int localLuminanceDownScale = 3;

                        // The luminance chain uses power-of-two intermediate targets, so it expects to output to one as well
                        var lumWidth = Math.Min(MathUtil.NextPowerOfTwo(currentInput.Size.Width), MathUtil.NextPowerOfTwo(currentInput.Size.Height));
                        lumWidth = Math.Max(1, lumWidth / 2);

                        var lumSize = new Size3(lumWidth, lumWidth, 1).Down2(localLuminanceDownScale);
                        luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                        luminanceEffect.SetOutput(luminanceTexture);
                    }

                    luminanceEffect.EnableLocalLuminanceCalculation = toneMap.UseLocalLuminance;
                    luminanceEffect.SetInput(currentInput);
                    luminanceEffect.Draw(context);

                    // Set this parameter that will be used by the tone mapping
                    colorTransformsGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
                }
            }

            if (BrightFilter.Enabled && (Bloom.Enabled || LightStreak.Enabled || LensFlare.Enabled))
            {
                Texture brightTexture = NewScopedRenderTarget2D(currentInput.Width, currentInput.Height, currentInput.Format, 1);
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.BrightFilter))
                {
                    // Bright filter pass

                    BrightFilter.SetInput(currentInput);
                    BrightFilter.SetOutput(brightTexture);
                    BrightFilter.Draw(context);
                }

                // Bloom pass
                if (Bloom.Enabled)
                {
                    using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.Bloom))
                    {
                        Bloom.SetInput(brightTexture);
                        Bloom.SetOutput(currentInput);
                        Bloom.Draw(context);
                    }
                }

                // Light streak pass
                if (LightStreak.Enabled)
                {
                    using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.LightStreak))
                    {
                        LightStreak.SetInput(brightTexture);
                        LightStreak.SetOutput(currentInput);
                        LightStreak.Draw(context);
                    }
                }

                // Lens flare pass
                if (LensFlare.Enabled)
                {
                    using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.LensFlare))
                    {
                        LensFlare.SetInput(brightTexture);
                        LensFlare.SetOutput(currentInput);
                        LensFlare.Draw(context);
                    }
                }
            }

            bool aaLast = needAA && !aaFirst;
            var toneOutput = aaLast ? NewScopedRenderTarget2D(input.Width, input.Height, input.Format) : output;

            // When FXAA is enabled we need to detect whether the ColorTransformGroup should output the Luminance into the alpha or not
            var luminanceToChannelTransform = colorTransformsGroup.PostTransforms.Get<LuminanceToChannelTransform>();
            if (fxaa != null)
            {
                if (luminanceToChannelTransform == null)
                {
                    luminanceToChannelTransform = new LuminanceToChannelTransform { ColorChannel = ColorChannel.A };
                    colorTransformsGroup.PostTransforms.Add(luminanceToChannelTransform);
                }

                // Only enabled when FXAA is enabled and InputLuminanceInAlpha is true
                luminanceToChannelTransform.Enabled = fxaa.Enabled && fxaa.InputLuminanceInAlpha;
            }
            else if (luminanceToChannelTransform != null)
            {
                luminanceToChannelTransform.Enabled = false;
            }

            using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.ColorTransformGroup))
            {
                // Color transform group pass (tonemap, color grading)
                var lastEffect = colorTransformsGroup.Enabled ? (ImageEffect)colorTransformsGroup : Scaler;
                lastEffect.SetInput(currentInput);
                lastEffect.SetOutput(toneOutput);
                lastEffect.Draw(context);
            }

            // do AA here, last, if not already done.
            if (aaLast)
            {
                using (context.QueryManager.BeginProfile(Color.Green, ImageEffectProfilingKeys.Fxaa))
                {
                    Antialiasing.SetInput(toneOutput);
                    Antialiasing.SetOutput(output);
                    Antialiasing.Draw(context);
                }
            }
        }
    }
}
