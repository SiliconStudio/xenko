using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    [DefaultPipelinePlugin(typeof(SkyboxPipelinePlugin))]
    public class RenderSkybox : RenderObject
    {
        public Skybox Skybox;
        public SkyboxBackground Background;
        public float Intensity;
        public Quaternion Rotation;

        internal SkyboxRenderFeature.SkyboxInfo SkyboxInfo;
    }
}