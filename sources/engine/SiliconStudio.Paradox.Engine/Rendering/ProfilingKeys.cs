// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Various <see cref="ProfilingKey"/> used to measure performance across some part of the effect system.
    /// </summary>
    public class ProfilingKeys
    {
        public static readonly ProfilingKey Engine = new ProfilingKey("Engine");

        public static readonly ProfilingKey ModelRenderProcessor = new ProfilingKey(Engine, "ModelRenderer");

        public static readonly ProfilingKey PrepareMesh = new ProfilingKey(ModelRenderProcessor, "PrepareMesh");

        public static readonly ProfilingKey RenderMesh = new ProfilingKey(ModelRenderProcessor, "RenderMesh");

        public static readonly ProfilingKey AnimationProcessor = new ProfilingKey(Engine, "AnimationProcessor");
    }
}