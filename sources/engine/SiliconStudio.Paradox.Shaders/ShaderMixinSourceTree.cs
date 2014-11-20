// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Contains a tree of <see cref="ShaderMixinSource"/>. 
    /// </summary>
    [DataContract]
    public sealed class ShaderMixinSourceTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinSourceTree"/> class.
        /// </summary>
        public ShaderMixinSourceTree()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinSource"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public ShaderMixinSourceTree(ShaderMixinSourceTree parent)
        {
            Parent = parent;
            Children = new Dictionary<string, ShaderMixinSourceTree>();
            Mixin = new ShaderMixinSource();
        }

        /// <summary>
        /// Gets or sets the name of the pdxfx effect linked to this node.
        /// </summary>
        /// <value>The name of the pdxfx effect.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mixin.
        /// </summary>
        /// <value>The mixin.</value>
        public ShaderMixinSource Mixin { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ShaderMixinSourceTree Parent { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>The children.</value>
        public Dictionary<string, ShaderMixinSourceTree> Children { get; set; }
    }
}