// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Effects
{
    internal static class MeshRenderExtensions
    {
        internal static ModelRendererState GetModelRendererState(this RenderPass pass)
        {
            return GetOrCreateModelRendererState(pass, false);
        }

        internal static ModelRendererState GetOrCreateModelRendererState(this RenderPass pass, bool createMeshStateIfNotFound = true)
        {
            var pipeline = pass.Pipeline;
            if (pipeline == null)
            {
                throw new ArgumentException("RenderPass is not associated with a RenderPipeline", "pass");
            }
            var pipelineState = pipeline.Tags.Get(ModelRendererState.Key);
            if (createMeshStateIfNotFound && pipelineState == null)
            {
                pipelineState = new ModelRendererState();
                pipeline.Tags.Set(ModelRendererState.Key, pipelineState);
            }
            return pipelineState;
        }
    }
}