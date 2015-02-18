// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
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
        }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(10)]
        [Category]
        public SceneGraphicsLayerCollection Layers { get; private set; }

        /// <summary>
        /// Gets the master layer.
        /// </summary>
        /// <value>The master layer.</value>
        [DataMember(20)]
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
            // Draw the layers
            Layers.Draw(context);

            // Draw the master track
            Master.Draw(context);
        }
    }
}