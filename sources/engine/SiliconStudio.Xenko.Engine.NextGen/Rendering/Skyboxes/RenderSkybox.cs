using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public class RenderSkybox : RenderObject
    {
        public bool Visible;
        public Skybox Skybox;
        public SkyboxBackground Background;
        public float Intensity;

        internal ValueParameter<float> RotationParameter;
        internal ValueParameter<Matrix> SkyMatrixParameter;
    }
}