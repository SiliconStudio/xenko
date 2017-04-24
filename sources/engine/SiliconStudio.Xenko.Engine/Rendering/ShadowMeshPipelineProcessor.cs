// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Pipline processor for <see cref="RenderMesh"/> that cast shadows, to properly disable culling and depth clip.
    /// </summary>
    public class ShadowMeshPipelineProcessor : PipelineProcessor
    {
        public RenderStage ShadowMapRenderStage { get; set; }

        [DefaultValue(false)]
        public bool DepthClipping { get; set; } = false;

        public override void Process(RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            // Objects in the shadow map render stage disable culling and depth clip
            if (renderNode.RenderStage == ShadowMapRenderStage)
            {
                pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.None) { DepthClipEnable = DepthClipping };
            }
        }
    }
}
