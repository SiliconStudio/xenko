using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    [DefaultPipelinePlugin(typeof(SkyboxPipelinePlugin))]
    public class RenderSkybox : RenderObject
    {
        public Skybox Skybox;
        public SkyboxBackground Background;

        internal SkyboxRenderFeature.SkyboxInfo SkyboxInfo;
    }
}