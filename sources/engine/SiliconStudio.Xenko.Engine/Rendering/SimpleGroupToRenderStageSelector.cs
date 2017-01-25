using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class SimpleGroupToRenderStageSelector : RenderStageSelector
    {
        public EntityGroup EntityGroup { get; set; }
        public RenderStage RenderStage { get; set; }
        public string EffectName { get; set; }

        public override void Process(RenderObject renderObject)
        {
            if (renderObject.RenderGroup == EntityGroup)
            {
                renderObject.ActiveRenderStages[RenderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}