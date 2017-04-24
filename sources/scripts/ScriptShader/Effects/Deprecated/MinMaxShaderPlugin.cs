// Copyright (c) 2011 Silicon Studio

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Light Shaft plugin.
    /// </summary>
    public class MinMaxShaderPlugin : ShaderPlugin<RenderPassPlugin>
    {
        private EffectShaderPass minPass;
        private EffectShaderPass maxPass;


        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public MinMaxShaderPlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public MinMaxShaderPlugin(string name)
            : base(name)
        {
            MinMaxShader = new ShaderClassSource("MinMaxBounding");
        }

        public ShaderClassSource MinMaxShader { get; set; }

        /// <param name="effectMesh"></param>
        /// <inheritdoc/>
        public override void SetupPasses(EffectMesh effectMesh)
        {
            // Special case: We have 2 shader passes that are sharing the same Shader, but with different parameters
            throw new System.NotImplementedException();
            //maxPass = CreateShaderPass(RenderPassPlugin.RenderPass, "MaxPass", RenderPassPlugin.Parameters);
            //minPass = CreateShaderPass(RenderPassPlugin.RenderPass, "MinPass", RenderPassPlugin.Parameters);
            DefaultShaderPass = maxPass;
        }

        /// <param name="effectMesh"></param>
        /// <inheritdoc/>
        public override void SetupShaders(EffectMesh effectMesh)
        {
            maxPass.Shader.Mixins.Add(MinMaxShader);
            minPass.Shader = maxPass.Shader;
        }

        public override void SetupResources(EffectMesh effectMesh)
        {
            base.SetupResources(effectMesh);

            Effect.Parameters.RegisterParameter(BlendStateKey);
            Effect.Parameters.RegisterParameter(RasterizerStateKey);

            // Create blendstate for min calculation
            var bbBlendDesc = new BlendStateDescription();
            bbBlendDesc.SetDefaults();
            bbBlendDesc.AlphaToCoverageEnable = false;
            bbBlendDesc.IndependentBlendEnable = false;
            bbBlendDesc.RenderTargets[0].BlendEnable = true;
            bbBlendDesc.RenderTargets[0].ColorSourceBlend = Blend.One;
            bbBlendDesc.RenderTargets[0].ColorDestinationBlend = Blend.One;

            bbBlendDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.Max;
            bbBlendDesc.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.Red;
            var blendMinState = BlendState.New(GraphicsDevice, bbBlendDesc);
            blendMinState.Name = "MinBlend";

            // Create blendstate for max calculation
            bbBlendDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.Max;
            bbBlendDesc.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.Green;
            var blendMaxState = BlendState.New(GraphicsDevice, bbBlendDesc);
            blendMaxState.Name= "MaxBlend";

            maxPass.Parameters.Set(RasterizerStateKey, GraphicsDevice.RasterizerStates.CullFront);
            maxPass.Parameters.Set(BlendStateKey, blendMaxState);

            minPass.Parameters.Set(RasterizerStateKey, GraphicsDevice.RasterizerStates.CullBack);
            minPass.Parameters.Set(BlendStateKey, blendMinState);
        }
    }
}