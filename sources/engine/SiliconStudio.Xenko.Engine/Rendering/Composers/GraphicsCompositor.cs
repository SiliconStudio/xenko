using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataSerializerGlobal(typeof(ReferenceSerializer<GraphicsCompositor>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<GraphicsCompositor>))]
    [DataContract]
    // Needed for indirect serialization of RenderSystem.RenderStages and RenderSystem.RenderFeatures
    // TODO: we would like an attribute to specify that serializing through the interface type is fine in this case (bypass type detection)
    [DataSerializerGlobal(null, typeof(FastTrackingCollection<RenderStage>))]
    [DataSerializerGlobal(null, typeof(FastTrackingCollection<RootRenderFeature>))]
    public class GraphicsCompositor
    {
        [Obsolete]
        public ISceneGraphicsCompositor Instance { get; set; }

        /// <summary>
        /// Gets the render system used with this graphics compositor.
        /// </summary>
        [DataMemberIgnore]
        public RenderSystem RenderSystem { get; } = new RenderSystem();

        /// <summary>
        /// The list of render stages.
        /// </summary>
        public IList<RenderStage> RenderStages => RenderSystem.RenderStages;

        /// <summary>
        /// The list of render features.
        /// </summary>
        public IList<RootRenderFeature> RenderFeatures => RenderSystem.RenderFeatures;

        /// <summary>
        /// The code and values defined by this graphics compositor.
        /// </summary>
        public IGraphicsCompositorTopPart TopLevel { get; set; }


        public void Draw(RenderDrawContext context)
        {
            if (TopLevel != null)
            {
                // Do we need to initialize render system?
                if (RenderSystem.GraphicsDevice == null)
                    RenderSystem.Initialize(context.RenderContext);

                // Get or create VisibilityGroup for this RenderSystem + SceneInstance
                var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
                var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

                using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentVisibilityGroup, visibilityGroup))
                using (context.RenderContext.PushTagAndRestore(SceneInstance.CurrentRenderSystem, RenderSystem))
                {
                    // Reset & cleanup
                    visibilityGroup.Reset();

                    // Clear views
                    foreach (var renderView in RenderSystem.Views)
                    {
                        renderView.RenderStages.Clear();
                    }
                    RenderSystem.Views.Clear();

                    // Set render system
                    context.RenderContext.RenderSystem = RenderSystem;
                    context.RenderContext.SceneInstance = sceneInstance;
                    context.RenderContext.VisibilityGroup = visibilityGroup;

                    // Set start states for viewports and output (it will be used during the Collect phase)
                    var renderOutputs = new RenderOutputDescription();
                    renderOutputs.CaptureState(context.CommandList);
                    context.RenderContext.RenderOutputs.Clear();
                    context.RenderContext.RenderOutputs.Push(renderOutputs);

                    var viewports = new ViewportState();
                    viewports.CaptureState(context.CommandList);
                    context.RenderContext.ViewportStates.Clear();
                    context.RenderContext.ViewportStates.Push(viewports);

                    try
                    {
                        // Collect in the game graphics compositor: Setup features/stages, enumerate views and populates VisibilityGroup
                        TopLevel.Collect(context.RenderContext);

                        // Collect in render features
                        RenderSystem.Collect(context.RenderContext);

                        // Extract
                        RenderSystem.Extract(context.RenderContext);

                        // Prepare
                        RenderSystem.Prepare(context);

                        // Draw using the game graphics compositor
                        TopLevel.Draw(context);

                        // Flush
                        RenderSystem.Flush(context);
                    }
                    finally
                    {
                        // Reset render context data
                        RenderSystem.Reset();
                    }
                }
            }
            else
            {
                Instance?.Draw(context);
            }
        }
    }
}