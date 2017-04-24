// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class WireframeShaderPlugin : ShaderPlugin<RenderPassPlugin>
    {
        /// <summary>
        /// Gets or sets the main target plugin this instance is attached to.
        /// </summary>
        /// <value>
        /// The main target plugin.
        /// </value>
        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        public override void SetupPasses(EffectMesh effectMesh)
        {
            base.SetupPasses(effectMesh);
        }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            // Duplicate the main shader
            throw new System.NotImplementedException();
            EffectShaderPass mainShaderPass;
            //var mainShaderPass = FindShaderPassFromPlugin(MainTargetPlugin);
            DefaultShaderPass.Shader = (ShaderMixinSource)mainShaderPass.Shader.Clone();
            DefaultShaderPass.Macros.AddRange(mainShaderPass.Macros);
            DefaultShaderPass.SubMeshDataKey = mainShaderPass.SubMeshDataKey;

            // Wireframe are white!
            var wireframeShader = new ShaderClassSource("Wireframe");
            DefaultShaderPass.Shader.Mixins.Add(wireframeShader);
        }

        public override void SetupResources(EffectMesh effectMesh)
        {
            base.SetupResources(effectMesh);

            throw new System.NotImplementedException();
            EffectShaderPass effectShaderPass;
            //var effectShaderPass = FindShaderPassFromPlugin(RenderPassPlugin);

            var rasterizer = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe, DepthBias = -1000 });
            rasterizer.Name = "WireFrame";
            effectShaderPass.Parameters.Set(RasterizerStateKey, rasterizer);

            var depthStencilState = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription(true, false) { DepthBufferFunction = CompareFunction.LessEqual });
            effectShaderPass.Parameters.Set(DepthStencilStateKey, depthStencilState);
        }
    }
}
