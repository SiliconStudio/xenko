using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class TransparentRenderStageSelector : RenderStageSelector
    {
        [DefaultValue(EntityGroupMask.Group0)]
        public EntityGroupMask EntityGroup { get; set; } = EntityGroupMask.Group0;

        public RenderStage MainRenderStage { get; set; }
        public RenderStage TransparentRenderStage { get; set; }

        public string EffectName { get; set; }
    }
}