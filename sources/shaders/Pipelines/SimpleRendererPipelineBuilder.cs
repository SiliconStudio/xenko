namespace SiliconStudio.Paradox.Effects.Pipelines
{
    public class SimpleRendererPipelineBuilder : PipelineBuilder
    {
        public Renderer Renderer { get; private set; }

        public SimpleRendererPipelineBuilder(Renderer renderer)
        {
            Renderer = renderer;
        }

        public override void Load()
        {
            AddRenderer(Renderer);
        }
    }
}