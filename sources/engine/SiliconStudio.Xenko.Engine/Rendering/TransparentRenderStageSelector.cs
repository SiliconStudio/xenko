namespace SiliconStudio.Xenko.Rendering
{
    public abstract class TransparentRenderStageSelector : RenderStageSelector
    {
        public RenderStage MainRenderStage { get; set; }
        public RenderStage TransparentRenderStage { get; set; }

        public string EffectName { get; set; }
    }
}