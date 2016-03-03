using System.Linq;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public static class NextGenRenderSystemExtensions
    {
        public static RenderStage GetRenderStage(this NextGenRenderSystem renderSystem, string name)
        {
            return renderSystem.RenderStages.FirstOrDefault(x => x.Name == name);
        }

        public static RenderStage GetOrCreateRenderStage(this NextGenRenderSystem renderSystem, string name, string effectSlotName, RenderOutputDescription defaultOutput)
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