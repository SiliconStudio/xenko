namespace SiliconStudio.Xenko.Rendering
{
    public class MeshTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            var renderMesh = (RenderMesh)renderObject;

            var renderStage = renderMesh.Material.HasTransparency ? TransparentRenderStage : MainRenderStage;
            renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
        }
    }
}