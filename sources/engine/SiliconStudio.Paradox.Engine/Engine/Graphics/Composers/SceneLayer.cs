// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A graphics layer.
    /// </summary>
    [DataContract("SceneLayer")]
    public class SceneLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneLayer"/> class.
        /// </summary>
        public SceneLayer()
        {
            Enabled = true;
            Mode = new GraphicsRenderingModeForward();
            Input = new GraphicsLayerInputNone();
            Output = new GraphicsComposerOutputMaster();
            Renderers = new GraphicsRendererCollection();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SceneLayer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(0)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this layer.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        /// <value>The mode.</value>
        /// <userdoc>Defines the rendering mode (Forward, Deferred...etc.)</userdoc>
        [DataMember(20)]
        [NotNull]
        public IGraphicsRenderingMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the input this layer.
        /// </summary>
        /// <value>The input.</value>
        /// <userdoc>
        /// Defines the input of a layer. This can be the previous layer or a specific layer or a render target...etc.
        /// </userdoc>
        [DataMember(30)]
        [NotNull]
        public IGraphicsLayerInput Input { get; set; }

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
        public IGraphicsComposerOutput Output { get; set; }

        /// <summary>
        /// Gets the renderers that will be used to render this layer.
        /// </summary>
        /// <value>The renderers.</value>
        /// <userdoc>
        /// The renderers that will be used to render this layer.
        /// </userdoc>
        [DataMember(50)]
        [Category]
        public GraphicsRendererCollection Renderers { get; private set; }
    }
}