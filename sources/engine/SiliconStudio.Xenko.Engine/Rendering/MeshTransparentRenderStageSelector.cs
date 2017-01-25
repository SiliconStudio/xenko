using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class MeshTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public EntityGroup EntityGroup { get; set; }

        public override void Process(RenderObject renderObject)
        {
            var entityGroup = renderObject.RenderGroup;

            if (entityGroup == EntityGroup)
            {
                var renderMesh = (RenderMesh)renderObject;

                var renderStage = renderMesh.Material.HasTransparency ? TransparentRenderStage : MainRenderStage;
                renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}