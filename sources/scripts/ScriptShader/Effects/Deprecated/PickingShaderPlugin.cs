// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class PickingShaderPlugin : ShaderPlugin<PickingPlugin>
    {
        public RenderPassPlugin MainPlugin { get; set; }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            // Duplicate the main shader
            throw new System.NotImplementedException();
            EffectShaderPass mainShaderPass;
            //var mainShaderPass = FindShaderPassFromPlugin(MainPlugin);
            DefaultShaderPass.Shader = (ShaderMixinSource)mainShaderPass.Shader.Clone();
            DefaultShaderPass.Macros.AddRange(mainShaderPass.Macros);
            DefaultShaderPass.SubMeshDataKey = mainShaderPass.SubMeshDataKey;

            BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, new ShaderClassSource("PickingRasterizer"), true);
        }
    }
}
