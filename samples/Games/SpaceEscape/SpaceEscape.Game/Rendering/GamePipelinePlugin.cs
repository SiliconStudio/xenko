using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SpaceEscape.Rendering
{
    /// <summary>
    /// Plugin for the graphics pipeline that automatically registers the <see cref="GameRenderFeature"/>.
    /// </summary>
    public class GamePipelinePlugin : PipelinePlugin<MeshRenderFeature>
    {
        private GameRenderFeature gameRenderFeature;

        [ModuleInitializer]
        internal static void Initialize()
        {
            // Register the plugin if the MeshPipelinePlugin is present.
            PipelinePluginManager.RegisterAutomaticPlugin(typeof(GamePipelinePlugin), typeof(MeshPipelinePlugin));
        }

        public override void Load(PipelinePluginContext context)
        {
            base.Load(context);

            // Add the custom render feature
            RenderFeature.RenderFeatures.Add(gameRenderFeature = new GameRenderFeature());
        }

        public override void Unload(PipelinePluginContext context)
        {
            // Cleanup render features
            RenderFeature.RenderFeatures.Remove(gameRenderFeature);

            base.Unload(context);
        }
    }
}