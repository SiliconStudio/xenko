using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public abstract class MainPipelineBuilder : PipelineBuilder
    {
        public PipelineBuilder BeforeMainRender { get; set; }

        public Color ClearColor { get; set; }

        public string EffectName { get; set; }
    }
}