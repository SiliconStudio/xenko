using System;

namespace SiliconStudio.Xenko.Rendering
{
    public class DefaultPipelinePluginAttribute : Attribute
    {
        public DefaultPipelinePluginAttribute(Type pipelinePluginType)
        {
            PipelinePluginType = pipelinePluginType;
        }

        public Type PipelinePluginType { get; }
    }
}