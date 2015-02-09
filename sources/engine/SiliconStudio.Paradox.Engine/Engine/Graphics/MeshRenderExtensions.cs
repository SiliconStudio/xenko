// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Paradox.Engine.Graphics.Composers;

namespace SiliconStudio.Paradox.Effects
{
    internal static class MeshRenderExtensions
    {
        internal static ModelRendererState GetModelRendererState(this SceneRenderer sceneRenderer)
        {
            return GetOrCreateModelRendererState(sceneRenderer, false);
        }

        internal static ModelRendererState GetOrCreateModelRendererState(this SceneRenderer sceneRenderer, bool createMeshStateIfNotFound = true)
        {
            var pipelineState = sceneRenderer.Tags.Get(ModelRendererState.Key);
            if (createMeshStateIfNotFound && pipelineState == null)
            {
                pipelineState = new ModelRendererState();
                sceneRenderer.Tags.Set(ModelRendererState.Key, pipelineState);
            }
            return pipelineState;
        }
    }
}