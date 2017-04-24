// Copyright (c) 2011 Silicon Studio

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Shader used with <see cref="LightingPrepassPlugin"/>.
    /// </summary>
    public class DeferredLightingShaderPlugin : ShaderPlugin<LightingPrepassPlugin>
    {
        public override void SetupPasses(EffectMesh effectMesh)
        {
        }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            // Merge only this shader on the main
            throw new System.NotImplementedException();
            EffectShaderPass mainShaderPass;
            //var mainShaderPass = FindShaderPassFromPlugin(RenderPassPlugin.GBufferPlugin.MainPlugin);
            mainShaderPass.Shader.Mixins.Add(new ShaderClassSource("LightDeferredShading"));
        }
    }
}