using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public class RenderSkybox : RenderObject
    {
        public Skybox Skybox;
        public SkyboxBackground Background;
        public float Intensity;

        // Used internally by renderer
        internal ValueParameter<float> RotationParameter;
        internal ValueParameter<Matrix> SkyMatrixParameter;
        internal DescriptorSetLayout DescriptorSetLayout;
        internal int ConstantBufferSize;
    }
}