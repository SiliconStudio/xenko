namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public class ShadowMapRenderStageSelector : RenderStageSelector
    {
        public RenderStage ShadowMapRenderStage { get; set; }
        public string EffectName { get; set; }

        public override void Process(RenderObject renderObject)
        {
            var renderMesh = (RenderMesh)renderObject;

            // Only handle non-transparent meshes
            if (!renderMesh.Material.HasTransparency)
            {
                if (renderMesh.IsShadowCaster)
                    renderMesh.ActiveRenderStages[ShadowMapRenderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}