using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;

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
            ambientOcclusion = new AmbientOcclusion();
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

        /// <inheritdoc/>
        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the ambient occlusion effect.
        /// </summary>
        /// <userdoc>
        /// The ambient occlusion post-effect allows you to simulate occlusion for opaque objects which are close to or occluded by other opaque objects.
        /// </userdoc>
        [DataMember(8)]
        [Category]
        public AmbientOcclusion AmbientOcclusion
        {
            get
            {
                return ambientOcclusion;
            }
        }

        /// <summary>
        /// Gets the depth of field effect.
        /// </summary>
        /// <value>The depth of field.</value>
        /// <userdoc>The depth of field post-effect allows you to accentuate some regions of your image by blurring object in foreground or background.</userdoc>
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
        /// <userdoc>The parameters for the bright filter. The bright filter is not an effect by itself. 
        /// It just extracts the brightest areas of the image and gives it to other effect that need it (eg. bloom, light streaks, lens-flares).</userdoc>
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
        /// <userdoc>Produces a bleeding effect of bright areas onto their surrounding.</userdoc>
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
        /// <userdoc>Produces a bleeding effect of the brightest points of the image along streaks.</userdoc>
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
        /// <userdoc>Simulates the artifacts produced by the internal reflection or scattering of the light within camera lens.</userdoc>
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
        /// <userdoc>Performs a transformation onto the image colors.</userdoc>
        [DataMember(70)]
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
        /// <userdoc>Performs anti-aliasing filtering on the image. This smoothes the jagged edges of models.</userdoc>
        [DataMember(80)]
        [Display("Type", "Antialiasing")]
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

        /// <summary>
        /// Disables all post processing effects.
        /// </summary>
        public void DisableAll()
        {
            ambientOcclusion.Enabled = false;
            depthOfField.Enabled = false;
            bloom.Enabled = false;
            lightStreak.Enabled = false;
            lensFlare.Enabled = false;
            ssaa.Enabled = false;
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

            ambientOcclusion = ToLoadAndUnload(ambientOcclusion);
            depthOfField = ToLoadAndUnload(depthOfField);
            luminanceEffect = ToLoadAndUnload(luminanceEffect);
            brightFilter = ToLoadAndUnload(brightFilter);
            bloom = ToLoadAndUnload(bloom);
            lightStreak = ToLoadAndUnload(lightStreak);
            lensFlare = ToLoadAndUnload(lensFlare);
            //this can be null if no SSAA is selected in the editor
            if(ssaa != null) ssaa = ToLoadAndUnload(ssaa);
            colorTransformsGroup = ToLoadAndUnload(colorTransformsGroup);
        }

        public void Collect(RenderContext context)
        {
        }

        public void Draw(RenderDrawContext drawContext, IRenderTarget inputTargetsComposition, Texture inputDepthStencil, Texture outputTarget)
        {
            var colorInput = inputTargetsComposition as IColorTarget;
            if (colorInput == null) return;

            SetInput(0, colorInput.Color);
            SetInput(1, inputDepthStencil);
            SetOutput(outputTarget);
            Draw(drawContext);
        }

        public bool RequiresVelocityBuffer => false;

        public bool RequiresNormalBuffer => false;

        protected override void DrawCore(RenderDrawContext context)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null || output == null)
            {
                return;
            }

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

            if (ambientOcclusion.Enabled && InputCount > 1 && GetInput(1) != null && GetInput(1).IsDepthStencil)
            {
                // Ambient Occlusion
                var aoOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                var inputDepthTexture = GetInput(1); // Depth
                ambientOcclusion.SetColorDepthInput(currentInput, inputDepthTexture);
                ambientOcclusion.SetOutput(aoOutput);
                ambientOcclusion.Draw(context);
                currentInput = aoOutput;
            }

            if (depthOfField.Enabled && InputCount > 1 && GetInput(1) != null && GetInput(1).IsDepthStencil)
            {
                // DoF
                var dofOutput = NewScopedRenderTarget2D(input.Width, input.Height, input.Format);
                var inputDepthTexture = GetInput(1); // Depth
                depthOfField.SetColorDepthInput(currentInput, inputDepthTexture);
                depthOfField.SetOutput(dofOutput);
                depthOfField.Draw(context);
                currentInput = dofOutput;
            }

            // Luminance pass (only if tone mapping is enabled)
            // TODO: This is not super pluggable to have this kind of dependencies. Check how to improve this
            var toneMap = colorTransformsGroup.Transforms.Get<ToneMap>();
            if (colorTransformsGroup.Enabled && toneMap != null && toneMap.Enabled)
            {
                const int localLuminanceDownScale = 3;

                // The luminance chain uses power-of-two intermediate targets, so it expects to output to one as well
                var lumWidth = Math.Min(MathUtil.NextPowerOfTwo(currentInput.Size.Width), MathUtil.NextPowerOfTwo(currentInput.Size.Height));
                lumWidth = Math.Max(1, lumWidth / 2);

                var lumSize = new Size3(lumWidth, lumWidth, 1).Down2(localLuminanceDownScale);
                var luminanceTexture = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float, 1);

                luminanceEffect.SetInput(currentInput);
                luminanceEffect.SetOutput(luminanceTexture);
                luminanceEffect.Draw(context);

                // Set this parameter that will be used by the tone mapping
                colorTransformsGroup.Parameters.Set(LuminanceEffect.LuminanceResult, new LuminanceResult(luminanceEffect.AverageLuminance, luminanceTexture));
            }

            if (brightFilter.Enabled && (bloom.Enabled || lightStreak.Enabled || lensFlare.Enabled))
            {
                // Bright filter pass
                Texture brightTexture = null;
                brightTexture = NewScopedRenderTarget2D(currentInput.Width, currentInput.Height, currentInput.Format, 1);

                brightFilter.SetInput(currentInput);
                brightFilter.SetOutput(brightTexture);
                brightFilter.Draw(context);

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
            }

            var outputForLastEffectBeforeAntiAliasing = output;

            if (ssaa != null && ssaa.Enabled)
            {
                outputForLastEffectBeforeAntiAliasing = NewScopedRenderTarget2D(output.Width, output.Height, output.Format);
            }

            // When FXAA is enabled we need to detect whether the ColorTransformGroup should output the Luminance into the alpha or not
            var fxaa = ssaa as FXAAEffect;
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

            // Color transform group pass (tonemap, color grading)
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