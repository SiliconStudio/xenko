// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataContract]
    public abstract class PipelineCompositorLayers : CompositorLayersBase
    {
        /// <summary>
        /// Gets the render system used with this pipeline.
        /// </summary>
        [DataMemberIgnore]
        public RenderSystem RenderSystem { get; } = new RenderSystem();

        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        /// <userdoc>The name of the effect to use to render models (a '.xksl' or '.xkfx' filename without the extension).</userdoc>
        [DataMember(10)]
        [DefaultValue(MeshPipelinePlugin.DefaultEffectName)]
        public string ModelEffect
        {
            // TODO: This is not a good extensibility point. Check how to improve this
            get { return RenderSystem.PipelinePlugins.GetPlugin<MeshPipelinePlugin>().ModelEffect; }
            set { RenderSystem.PipelinePlugins.GetPlugin<MeshPipelinePlugin>().ModelEffect = value; }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        protected override void Destroy()
        {
            RenderSystem.Dispose();

            base.Destroy();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Get or create VisibilityGroup for this RenderSystem
            var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
            var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentVisibilityGroup, visibilityGroup))
            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentRenderSystem, RenderSystem))
            {
                // Collect
                visibilityGroup.Reset();

                try
                {
                    // Collect in layers. Setup features/stages, enumerate viewes and populates VisibilityGroup
                    Layers.Collect(context.RenderContext);
                    Master.Collect(context.RenderContext);

                    // Collect in render features
                    RenderSystem.Collect(context);

                    // Extract
                    RenderSystem.Extract(context);

                    // Prepare
                    RenderSystem.Prepare(context);

                    // Draw the layers
                    Layers.Draw(context);

                    // Draw the master track
                    Master.Draw(context);
                }
                finally
                {
                    // Reset render context data
                    RenderSystem.Reset();
                }
            }
        }
    }
}