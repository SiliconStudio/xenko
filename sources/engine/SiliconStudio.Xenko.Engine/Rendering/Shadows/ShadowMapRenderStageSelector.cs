using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public class ShadowMapRenderStageSelector : RenderStageSelector
    {
        public RenderStage ShadowMapRenderStage { get; set; }
        public string EffectName { get; set; }

        [DefaultValue(EntityGroupMask.Group0)]
        public EntityGroupMask EntityGroup { get; set; } = EntityGroupMask.Group0;

        public override void Process(RenderObject renderObject)
        {
            if (((EntityGroupMask)(1U << (int)renderObject.RenderGroup) & EntityGroup) != 0)
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
}