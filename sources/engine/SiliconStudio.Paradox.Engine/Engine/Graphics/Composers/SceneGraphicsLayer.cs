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
    [DataContract("SceneGraphicsLayer")]
    public class SceneGraphicsLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsLayer"/> class.
        /// </summary>
        public SceneGraphicsLayer()
        {
            Enabled = true;
            Input = new SceneGraphicsLayerInputNone();
            Output = new SceneGraphicsComposerOutputMaster();
            Renderers = new SceneRendererCollection();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SceneGraphicsLayer"/> is enabled.
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
        /// Gets or sets the input this layer.
        /// </summary>
        /// <value>The input.</value>
        /// <userdoc>
        /// Defines the input of a layer. This can be the previous layer or a specific layer or a render target...etc.
        /// </userdoc>
        [DataMember(30)]
        [NotNull]
        public ISceneGraphicsLayerInput Input { get; set; }

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
        public ISceneGraphicsComposerOutput Output { get; set; }

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
    }
}