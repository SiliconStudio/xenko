// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Rendering
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
