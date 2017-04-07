using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A renderer to resolve MSAA textures.
    /// </summary>
    [Display("MSAA Resolver")]
    public class MSAAResolver : ImageEffect
    {
        private readonly ImageEffectShader msaaResolver;
        private int maxSamples;

        /// <summary>
        /// MSAA resolve shader modes.
        /// </summary>
        public enum FilterTypes
        {
            /// <summary>
            /// Default filter
            /// </summary>
            Default = 0,

            /// <summary>
            /// Box filter.
            /// </summary>
            Box = 1,

            /// <summary>
            /// Triangle filter.
            /// </summary>
            Triangle = 2,

            /// <summary>
            /// Gaussian filter.
            /// </summary>
            Gaussian = 3,

            /// <summary>
            /// Blackman Harris filter.
            /// </summary>
            BlackmanHarris = 4,

            /// <summary>
            /// Smoothstep function filter.
            /// </summary>
            SmoothStep = 5,

            /// <summary>
            /// B-Spline filter.
            /// </summary>
            BSpline = 6,

            /// <summary>
            /// Catmull Rom filter.
            /// </summary>
            CatmullRom = 7,

            /// <summary>
            /// Mitchell filter.
            /// </summary>
            Mitchell = 8,

            /// <summary>
            /// Sinus function filter.
            /// </summary>
            Sinc = 9,
        }

        /// <summary>
        /// MSAA resolve filter type.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(FilterTypes.BSpline)]
        public FilterTypes FilterType { get; set; }

        /// <summary>
        /// MSAA resolve filter radius value.
        /// </summary>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.5, 3.0, 0.01, 0.1)]
        public float FilterRadius { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAAResolver"/> class.
        /// </summary>
        public MSAAResolver()
            : this("MSAAResolverEffect")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAAResolver"/> class.
        /// </summary>
        /// <param name="msaaResolverShaderName">Name of the bright pass shader.</param>
        public MSAAResolver(string msaaResolverShaderName)
            : base(msaaResolverShaderName)
        {
            if (msaaResolverShaderName == null) throw new ArgumentNullException(nameof(msaaResolverShaderName));
            msaaResolver = new ImageEffectShader(msaaResolverShaderName);

            FilterType = FilterTypes.BSpline;
            FilterRadius = 1.0f;
        }

        public void Resolve(RenderDrawContext drawContext, Texture input, Texture output, int maxResolveSamples)
        {
            // Force to resolve multi-sampled depth texture using only single sample
            if (input.IsDepthStencil)
                maxResolveSamples = 1;

            maxSamples = maxResolveSamples;
            SetInput(0, input);
            SetOutput(output);
            Draw(drawContext);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            ToLoadAndUnload(msaaResolver);
        }

        protected override void DrawCore(RenderDrawContext drawContext)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (!input.IsMultisample)
                throw new ArgumentOutOfRangeException(nameof(input), "Source texture is not a MSAA texture.");

            // Prepare
            int samplesCount = Math.Min(maxSamples, (int)input.MultisampleCount);
            var inputSize = input.Size;
            // SvPosUnpack = float4(float2(0.5, -0.5) * TextureSize, float2(0.5, 0.5) * TextureSize))
            // TextureSizeLess1 = TextureSize - 1
            msaaResolver.Parameters.Set(MSAAResolverShaderKeys.SvPosUnpack, new Vector4(0.5f * inputSize.Width, -0.5f * inputSize.Height, 0.5f * inputSize.Width, 0.5f * inputSize.Height));
            msaaResolver.Parameters.Set(MSAAResolverShaderKeys.TextureSizeLess1, new Vector2(inputSize.Width - 1.0f, inputSize.Height - 1.0f));
            msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterDiameter, FilterRadius * 2.0f);
            msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, samplesCount);
            
            if (FilterType == FilterTypes.Default)
            {
                // Resolve using in-build API function
                drawContext.CommandList.CopyMultisample(input, 0, output, 0);
            }
            else
            {
                // Resolve using custom pixel shader
                msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                msaaResolver.Parameters.Set(MSAAResolverParams.InputQuality, (int)input.MultisampleCount);
                if (samplesCount > 1)
                    msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterType, (int)FilterType);
                msaaResolver.SetOutput(output);
                msaaResolver.Draw(drawContext);
            }
        }
    }
}
