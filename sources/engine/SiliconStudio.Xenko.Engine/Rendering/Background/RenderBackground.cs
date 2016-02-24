using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Background
{
    [PipelineRenderer(typeof(BackgroundPipelineRenderer))]
    public class RenderBackground : RenderObject
    {
        public Texture Texture;
        public float Intensity;

        // Used internally by renderer
        internal ResourceGroupLayout ResourceGroupLayout;
        internal ResourceGroup Resources;
    }
}