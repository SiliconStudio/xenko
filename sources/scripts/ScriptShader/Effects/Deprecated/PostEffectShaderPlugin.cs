// Copyright (c) 2011 Silicon Studio

namespace SiliconStudio.Xenko.Rendering
{
    public class PostEffectShaderPlugin : PostEffectSeparateShaderPlugin
    {
        public override void SetupPasses(EffectMesh effectMesh)
        {
            var parameters = RenderPass != null && RenderPass.Parameters != null ? RenderPass.Parameters : RenderPassPlugin != null ? RenderPassPlugin.Parameters : null;
            //DefaultShaderPass = CreateShaderPass(null, "PostEffect", parameters);
            throw new System.NotImplementedException();
        }
    }
}
