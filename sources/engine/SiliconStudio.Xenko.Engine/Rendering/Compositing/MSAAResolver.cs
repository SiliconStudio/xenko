using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
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
            Smoothstep = 5,

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
        /// MSAA resolve filter type
        /// </summary>
        [DataMember(10)]
        [DefaultValue(FilterTypes.BSpline)]
        public FilterTypes FilterType { get; set; }

        /// <summary>
        /// MSAA resolve filter diameter value
        /// </summary>
        [DataMember(20)]
        [DefaultValue(2.0f)]
        [DataMemberRange(1.0, 6.0, 0.01, 0.1)]
        public float FilterDiameter { get; set; }

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
            FilterDiameter = 2.0f;
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
            if (!input.IsMultiSample)
                throw new ArgumentOutOfRangeException(nameof(input), "Source texture is not a MSAA texture.");

            // Prepare
            var inputSize = input.Size;
            // SvPosUnpack = float4(float2(0.5, -0.5) * TextureSize, float2(0.5, 0.5) * TextureSize))
            // TextureSizeLess1 = TextureSize - 1
            msaaResolver.Parameters.Set(MSAAResolverShaderKeys.SvPosUnpack, new Vector4(0.5f * inputSize.Width, -0.5f * inputSize.Height, 0.5f * inputSize.Width, 0.5f * inputSize.Height));
            msaaResolver.Parameters.Set(MSAAResolverShaderKeys.TextureSizeLess1, new Vector2(inputSize.Width - 1.0f, inputSize.Height - 1.0f));
            msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterDiameter, FilterDiameter);

            // Check if it's a depth buffer
            if (input.IsDepthStencil)
            {
                // Resolve multi-sampled depth texture but use only single sample
                msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, 1);
                msaaResolver.SetOutput(output);
                msaaResolver.Draw(drawContext);
            }
            else if (FilterType == FilterTypes.Default)
            {
                // Resolve using in-build API function
                drawContext.CommandList.CopyMultiSample(input, 0, output, 0);
            }
            else
            {
                // Resolve using custom pixel shader
                msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, (int)input.MultiSampleLevel);
                msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterType, (int)FilterType);
                msaaResolver.SetOutput(output);
                msaaResolver.Draw(drawContext);
            }
        }
    }
}
