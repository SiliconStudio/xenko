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

        // Used internally by renderer
        internal ResourceGroupLayout ResourceGroupLayout;
        internal ResourceGroup Resources;
        internal ParameterCollection IrradianceParameters;
        internal ParameterCollection.CompositionCopier IrradianceCopier;
        internal ParameterCollectionLayout ParameterCollectionLayout;
    }
}