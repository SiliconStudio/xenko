using System;

namespace SiliconStudio.Xenko.Rendering
{
    public class PipelineRendererAttribute : Attribute
    {
        public PipelineRendererAttribute(Type rendererType)
        {
            RendererType = rendererType;
        }

        public Type RendererType { get; }
    }
}