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
            if (!renderMesh.Material.Material.HasTransparency)
            {
                if (renderMesh.Material.IsShadowCaster)
                    renderMesh.ActiveRenderStages[ShadowMapRenderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}