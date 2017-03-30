using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract(Inherited = true)]
    public abstract class PipelineProcessor
    {
        public abstract void Process(RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState);
    }
}