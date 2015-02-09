// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("GraphicsComposerLayer")]
    [Display("Layer")]
    public sealed class GraphicsComposerLayer : GraphicsLayer, IGraphicsComposer
    {
        public GraphicsComposerLayer()
        {
            Layers = new GraphicsRendererCollection();
            Input = GraphicsLayerInputLayer.PreviousLayer();
        }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(45)]
        [Category]
        public GraphicsRendererCollection Layers { get; private set; }
    }
}