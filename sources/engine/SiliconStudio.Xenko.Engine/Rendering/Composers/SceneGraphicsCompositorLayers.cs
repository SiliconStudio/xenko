// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

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

        protected override void Unload()
        {
            Layers.Dispose();
            Master.Dispose();

            base.Unload();
        }

        public void BeforeExtract(RenderContext context)
        {
            using (context.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
            {
                // Draw the layers
                Layers.BeforeExtract(context);

                // Draw the master track
                Master.BeforeExtract(context);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            using (context.RenderContext.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
            {
                // Draw the layers
                Layers.Draw(context);

                // Draw the master track
                Master.Draw(context);
            }
        }
    }
}