namespace SiliconStudio.Xenko.Rendering
{
    public class SimpleGroupToRenderStageSelector : RenderStageSelector
    {
        public RenderStage RenderStage { get; set; }
        public string EffectName { get; set; }

        public override void Process(RenderObject renderObject)
        {
            renderObject.ActiveRenderStages[RenderStage.Index] = new ActiveRenderStage(EffectName);
        }
    }
}