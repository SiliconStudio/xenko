// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A graphics layer.
    /// </summary>
    [DataContract("SceneGraphicsLayer")]
    public class SceneGraphicsLayer : RendererBase
    {
        private IGraphicsLayerOutput previousOutput;

        /// <summary>
        /// Property key to access the Master <see cref="RenderFrame"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<RenderFrame> Master = new PropertyKey<RenderFrame>("RenderFrame.Master", typeof(RenderFrame));

        /// <summary>
        /// Property key to access the Input <see cref="RenderFrame"/> from the current SceneGraphicsLayer
        /// </summary>
        public static readonly PropertyKey<RenderFrame> CurrentInput = new PropertyKey<RenderFrame>("SceneGraphicsLayer.CurrentInput", typeof(SceneGraphicsLayer));

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsLayer"/> class.
        /// </summary>
        public SceneGraphicsLayer()
        {
            Enabled = true;
            Output = new GraphicsLayerOutputMaster();
            Renderers = new SceneRendererCollection();
        }

        /// <summary>
        /// Gets or sets the name of this layer.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the output of this layer.
        /// </summary>
        /// <value>The output.</value>
        /// <userdoc>
        /// Defines the output of a layer. This can be a local or shared render target.
        /// (This can be the previous layer or a specific layer or a render target...etc.)
        /// </userdoc>
        [DataMember(40)]
        [NotNull]
        public IGraphicsLayerOutput Output { get; set; }

        /// <summary>
        /// Gets the renderers that will be used to render this layer.
        /// </summary>
        /// <value>The renderers.</value>
        /// <userdoc>
        /// The renderers that will be used to render this layer.
        /// </userdoc>
        [DataMember(50)]
        [Category]
        public SceneRendererCollection Renderers { get; private set; }

        internal bool IsMaster { get; set; }

        protected override void Unload()
        {
            // Dispose the output
            if (Output != null)
            {
                Output.Dispose();
            }

            foreach (var renderer in Renderers)
            {
                renderer.Dispose();
            }

            base.Unload();
        }

        protected override void DrawCore(RenderContext context)
        {
            if (!Enabled || Output == null)
            {
                return;
            }

            // Sets the input of the layer (== last Current)
            var currentRenderFrame = context.Tags.Get(RenderFrame.Current);
            RenderFrame renderFrame;

            // Sets the output of the layer 
            // Master is always going to use the Master frame for the current frame.
            if (IsMaster)
            {
                renderFrame = context.Tags.Get(Master);
            }
            else
            {
                renderFrame = Output.GetRenderFrame(context);
            }

            using (var t1 = context.PushTagAndRestore(SceneGraphicsLayer.CurrentInput, currentRenderFrame))
            using (var t2 = context.PushTagAndRestore(RenderFrame.Current, renderFrame))
                Renderers.Draw(context);
        }
    }
}