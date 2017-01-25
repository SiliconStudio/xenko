using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class SimpleGroupToRenderStageSelector : RenderStageSelector
    {
        [DefaultValue(EntityGroupMask.Group0)]
        public EntityGroupMask EntityGroup { get; set; } = EntityGroupMask.Group0;

        public RenderStage RenderStage { get; set; }

        public string EffectName { get; set; }

        public override void Process(RenderObject renderObject)
        {
            if (((EntityGroupMask)(1U << (int)renderObject.RenderGroup) & EntityGroup) != 0)
            {
                renderObject.ActiveRenderStages[RenderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}