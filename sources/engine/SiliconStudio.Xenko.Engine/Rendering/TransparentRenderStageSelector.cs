using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class TransparentRenderStageSelector : RenderStageSelector
    {
        [DefaultValue(RenderGroupMask.Group0)]
        public RenderGroupMask RenderGroup { get; set; } = RenderGroupMask.Group0;

        public RenderStage MainRenderStage { get; set; }
        public RenderStage TransparentRenderStage { get; set; }

        public string EffectName { get; set; }
    }
}