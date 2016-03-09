// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public static class RenderSystemExtensions
    {
        public static RenderStage GetRenderStage(this RenderSystem renderSystem, string name)
        {
            return renderSystem.RenderStages.FirstOrDefault(x => x.Name == name);
        }

        public static RenderStage GetOrCreateRenderStage(this RenderSystem renderSystem, string name, string effectSlotName, RenderOutputDescription defaultOutput)
        {
            var renderStage = renderSystem.RenderStages.FirstOrDefault(x => x.Name == name);
            if (renderStage != null)
                return renderStage;

            renderStage = new RenderStage(name, effectSlotName) { Output = defaultOutput };
            renderSystem.RenderStages.Add(renderStage);

            return renderStage;
        }
    }
}