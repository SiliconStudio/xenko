// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Used to express shadows are active.
    /// </summary>
    public class ShadowPipelinePlugin : IPipelinePlugin
    {
        public HashSet<RenderView> RenderViewsWithShadows { get; } = new HashSet<RenderView>();

        public void Load(PipelinePluginContext context)
        {
        }

        public void Unload(PipelinePluginContext context)
        {
        }
    }
}