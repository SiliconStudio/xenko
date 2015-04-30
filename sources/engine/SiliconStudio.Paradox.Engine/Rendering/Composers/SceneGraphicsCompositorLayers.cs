// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering.Composers
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
            Master = new SceneGraphicsLayer()
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
        [DataMember(10)]
        [Category]
        public SceneCameraSlotCollection Cameras { get; private set; }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(20)]
        [Category]
        public SceneGraphicsLayerCollection Layers { get; private set; }

        /// <summary>
        /// Gets the master layer.
        /// </summary>
        /// <value>The master layer.</value>
        [DataMember(30)]
        [Category]
        public SceneGraphicsLayer Master { get; private set; }

        protected override void Unload()
        {
            Layers.Dispose();
            Master.Dispose();

            base.Unload();
        }

        protected override void DrawCore(RenderContext context)
        {
            using (var t1 = context.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
            {
                // Draw the layers
                Layers.Draw(context);

                // Draw the master track
                Master.Draw(context);
            }
        }
    }
}