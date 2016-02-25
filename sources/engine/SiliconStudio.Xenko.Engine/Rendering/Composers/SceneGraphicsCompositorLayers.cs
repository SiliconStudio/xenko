// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("SceneGraphicsCompositorLayers")]
    [Display("Layers")]
    public sealed class SceneGraphicsCompositorLayers : RendererBase, ISceneGraphicsCompositor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsCompositorLayers"/> class.
        /// </summary>
        public SceneGraphicsCompositorLayers()
        {
            Layers = new SceneGraphicsLayerCollection();
            Master = new SceneGraphicsLayer
            {
                Output = new MasterRenderFrameProvider(),
                IsMaster = true
            };
            Cameras = new SceneCameraSlotCollection();
        }

        [DataMemberIgnore]
        public NextGenRenderSystem RenderSystem { get; } = new NextGenRenderSystem();

        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [DataMember(10)]
        [Category]
        public SceneCameraSlotCollection Cameras { get; private set; }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        /// <userdoc>The sequence of graphic layers to incorporate into the pipeline</userdoc>
        [DataMember(20)]
        [Category]
        [MemberCollection(CanReorderItems = true)]
        public SceneGraphicsLayerCollection Layers { get; private set; }

        /// <summary>
        /// Gets the master layer.
        /// </summary>
        /// <value>The master layer.</value>
        /// <userdoc>The main layer of the pipeline. Its output is the window back buffer.</userdoc>
        [DataMember(30)]
        [Category]
        public SceneGraphicsLayer Master { get; private set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        protected override void Unload()
        {
            Layers.Dispose();
            Master.Dispose();

            base.Unload();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Get or create VisibilityGroup for this RenderSystem
            var sceneInstance = SceneInstance.GetCurrent(context.RenderContext);
            var visibilityGroup = sceneInstance.GetOrCreateVisibilityGroup(RenderSystem);

            using (context.RenderContext.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
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