using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class MeshTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            if (((EntityGroupMask)(1U << (int)renderObject.RenderGroup) & EntityGroup) != 0)
            {
                var renderMesh = (RenderMesh)renderObject;

                var renderStage = renderMesh.Material.HasTransparency ? TransparentRenderStage : MainRenderStage;
                renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}