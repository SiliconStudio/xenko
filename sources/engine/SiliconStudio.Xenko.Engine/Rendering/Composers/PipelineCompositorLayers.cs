// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        public NextGenRenderSystem RenderSystem { get; } = new NextGenRenderSystem();

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Get or create VisibilityGroup for this RenderSystem
            var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
            var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentVisibilityGroup, visibilityGroup))
            using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentRenderSystem, RenderSystem))
            {
                // Draw the layers
                Layers.BeforeExtract(context.RenderContext);

                // Draw the master track
                Master.BeforeExtract(context.RenderContext);

                // Update current camera to render view
                foreach (var mainRenderView in RenderSystem.Views)
                {
                    if (mainRenderView.GetType() == typeof(RenderView))
                    {
                        RenderSystem.UpdateCameraToRenderView(context, mainRenderView);
                    }
                }

                // Collect
                // TODO GRAPHICS REFACTOR choose which views to collect
                visibilityGroup.Views.AddRange(RenderSystem.Views);
                visibilityGroup.Collect();
                visibilityGroup.Views.Clear();

                // Extract
                RenderSystem.Extract(context);

                // Prepare
                RenderSystem.Prepare(context);

                // Draw the layers
                Layers.Draw(context);

                // Draw the master track
                Master.Draw(context);

                // Reset render context data
                RenderSystem.Reset();
            }
        }
    }
}