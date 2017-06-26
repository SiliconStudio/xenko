// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
        private readonly ImageEffectShader msaaDepthResolver;

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
        [DataMemberRange(0.5, 3.0, 0.01, 0.1, 3)]
        public float FilterRadius { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAAResolver"/> class.
        /// </summary>
        public MSAAResolver()
            : this("MSAAResolverEffect", "MSAADepthResolverEffect")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSAAResolver"/> class.
        /// </summary>
        /// <param name="msaaResolverShaderName">Name of the MSAA resolve pass shader.</param>
        /// <param name="msaaDepthResolverShaderName">Name of the MSAA depth resolve pass shader.</param>
        public MSAAResolver(string msaaResolverShaderName, string msaaDepthResolverShaderName)
            : base(msaaResolverShaderName)
        {
            if (msaaResolverShaderName == null) throw new ArgumentNullException(nameof(msaaResolverShaderName));
            if (msaaDepthResolverShaderName == null) throw new ArgumentNullException(nameof(msaaDepthResolverShaderName));

            msaaResolver = new ImageEffectShader(msaaResolverShaderName);
            msaaDepthResolver = new ImageEffectShader(msaaDepthResolverShaderName);

            FilterType = FilterTypes.BSpline;
            FilterRadius = 1.0f;

            EnableSetRenderTargets = false;
        }

        /// <summary>
        /// Resolves the specified input multisampled texture.
        /// </summary>
        /// <param name="drawContext">The draw context.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public void Resolve(RenderDrawContext drawContext, Texture input, Texture output)
        {
            SetInput(0, input);
            SetOutput(output);
            Draw(drawContext);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            ToLoadAndUnload(msaaResolver);
            ToLoadAndUnload(msaaDepthResolver);
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
            if (output.IsMultisample)
                throw new ArgumentOutOfRangeException(nameof(input), "Destination texture is a MSAA texture.");

            // Prepare
            int samplesCount = (int)input.MultisampleCount;
            var inputSize = input.Size;
            // SvPosUnpack = float4(float2(0.5, -0.5) * TextureSize, float2(0.5, 0.5) * TextureSize))
            // TextureSizeLess1 = TextureSize - 1
            var svPosUnpack = new Vector4(0.5f * inputSize.Width, -0.5f * inputSize.Height, 0.5f * inputSize.Width, 0.5f * inputSize.Height);
            var textureSizeLess1 = new Vector2(inputSize.Width - 1.0f, inputSize.Height - 1.0f);

            if (input.IsDepthStencil)
            {
                System.Diagnostics.Debug.Assert(output.IsDepthStencil);

                // Resolve using custom pixel shader (output depth only)
                msaaDepthResolver.Parameters.Set(MSAAResolverParams.MSAASamples, samplesCount);
                msaaDepthResolver.Parameters.Set(MSAADepthResolverShaderKeys.SvPosUnpack, svPosUnpack);
                msaaDepthResolver.Parameters.Set(MSAADepthResolverShaderKeys.TextureSizeLess1, textureSizeLess1);
                msaaDepthResolver.Parameters.Set(MSAADepthResolverShaderKeys.InputTexture, input);
                msaaDepthResolver.SetDepthOutput(output, (Texture)null);
                msaaDepthResolver.Draw(drawContext);
            }
            else
            {
                if (FilterType == FilterTypes.Default)
                {
                    // Resolve using in-build API function
                    drawContext.CommandList.CopyMultisample(input, 0, output, 0);
                }
                else
                {
                    // Resolve using custom pixel shader
                    msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, samplesCount);
                    msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterDiameter, FilterRadius * 2.0f);
                    msaaResolver.Parameters.Set(MSAAResolverShaderKeys.SvPosUnpack, svPosUnpack);
                    msaaResolver.Parameters.Set(MSAAResolverShaderKeys.TextureSizeLess1, textureSizeLess1);
                    msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                    if (samplesCount > 1)
                        msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterType, (int)FilterType);
                    msaaResolver.SetOutput(output);
                    msaaResolver.Draw(drawContext);
                }
            }
        }
    }
}
