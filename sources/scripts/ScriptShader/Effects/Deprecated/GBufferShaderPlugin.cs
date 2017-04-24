// Copyright (c) 2011 Silicon Studio

using System.Linq;

using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Shader effect used with <see cref="GBufferPlugin"/>
    /// </summary>
    public class GBufferShaderPlugin : ShaderPlugin<GBufferPlugin>
    {
        [EffectDefinitionProperty]
        public ShaderSource Mixin { get; set; }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            // Duplicate the main shader
            throw new System.NotImplementedException();
            EffectShaderPass mainShaderPass;
            //var mainShaderPass = FindShaderPassFromPlugin(RenderPassPlugin.MainTargetPlugin);
            DefaultShaderPass.Shader = (ShaderMixinSource)mainShaderPass.Shader.Clone();
            DefaultShaderPass.Macros.AddRange(mainShaderPass.Macros);
            DefaultShaderPass.SubMeshDataKey = mainShaderPass.SubMeshDataKey;

            // G-Buffer construction
            // ExtractGBuffer: Extract POSITION + NORMAL
            DefaultShaderPass.Shader.Mixins.Add("GBuffer");
            DefaultShaderPass.Shader.Mixins.Add("NormalVSStream");

            mainShaderPass.Shader.Mixins.Remove("PositionVSStream");
            mainShaderPass.Shader.Mixins.Remove("NormalVSStream");
            mainShaderPass.Shader.Mixins.Add("NormalVSGBuffer");
            mainShaderPass.Shader.Mixins.Add("SpecularPowerGBuffer");
            mainShaderPass.Shader.Mixins.Add("PositionVSGBuffer");

            // Apply Mixin
            if (Mixin != null)
                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, Mixin, true);
        }

        public override void SetupResources(EffectMesh effectMesh)
        {
            base.SetupResources(effectMesh);
            Effect.PrepareMesh += SetupMeshResources;
        }

        private void SetupMeshResources(EffectOld effect, EffectMesh effectMesh)
        {
            effectMesh.Parameters.Set(DepthStencilStateKey, RenderPassPlugin.DepthStencilStateZReadOnly);
        }
    }
}