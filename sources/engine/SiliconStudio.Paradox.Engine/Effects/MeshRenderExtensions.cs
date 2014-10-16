// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Effects
{
    public static class MeshRenderExtensions
    {
        public static MeshRenderState GetMeshRenderState(this RenderPass pass)
        {
            return GetOrCreateMeshRenderState(pass, false);
        }

        public static MeshRenderState GetOrCreateMeshRenderState(this RenderPass pass, bool createMeshStateIfNotFound = true)
        {
            var pipeline = pass.Pipeline;
            if (pipeline == null)
            {
                throw new ArgumentException("RenderPass is not associated with a RenderPipeline", "pass");
            }
            var pipelineState = pipeline.Tags.Get(MeshRenderState.Key);
            if (createMeshStateIfNotFound && pipelineState == null)
            {
                pipelineState = new MeshRenderState();
                pipeline.Tags.Set(MeshRenderState.Key, pipelineState);
            }
            return pipelineState;
        }
    }
}