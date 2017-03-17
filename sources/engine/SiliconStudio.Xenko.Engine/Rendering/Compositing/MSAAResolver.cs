using System;
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
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            ToLoadAndUnload(msaaResolver);
        }

        protected override void SetDefaultParameters()
        {
            ResolveFilterDiameter = 2.0f;

            base.SetDefaultParameters();
        }

        /// <summary>
        /// MSAA resolve filter diameter value
        /// </summary>
        [DataMember]
        [DataMemberRange(1.0, 6.0, 0.01, 0.1)]
        public float ResolveFilterDiameter { get; set; } = 2.0f;
        
        protected override void DrawCore(RenderDrawContext drawContext)
        {
            var input = GetInput(0);
            var output = GetOutput(0);
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            // Prepare
            var inputSize = input.Size;
            msaaResolver.Parameters.Set(MSAAResolverShaderKeys.TextureSize, new Vector2(inputSize.Width, inputSize.Height));
            msaaResolver.Parameters.Set(MSAAResolverParams.ResolveFilterDiameter, ResolveFilterDiameter);

            // Check if it's a depth buffer
            if (input.IsDepthStencil)
            {
                // Resolve multi-sampled depth texture but use only single sample
                msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, 1);
                msaaResolver.SetOutput(output);
                msaaResolver.Draw(drawContext);
            }
            else
            {
                // Resolve using in-build API
                //drawContext.CommandList.CopyMultiSample(input, 0, output, 0);

                // Resolve using custom pixel shader
                msaaResolver.Parameters.Set(MSAAResolverShaderKeys.InputTexture, input);
                msaaResolver.Parameters.Set(MSAAResolverParams.MSAASamples, (int)input.MultiSampleLevel);
                msaaResolver.SetOutput(output);
                msaaResolver.Draw(drawContext);
            }
        }
    }
}