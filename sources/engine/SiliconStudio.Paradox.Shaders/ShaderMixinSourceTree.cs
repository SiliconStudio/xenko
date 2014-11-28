// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

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

        /// <summary>
        /// Gets the used parameters for this mixin tree.
        /// </summary>
        /// <value>The used parameters.</value>
        [DataMemberIgnore]
        public ShaderMixinParameters UsedParameters { get; set; }

        /// <summary>
        /// Gets the fullname using parents name.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            // TODO: method not optimal, but only used for debugging
            var tree = this;
            var list = new Stack<string>();
            while (tree != null)
            {
                list.Push(tree.Name);
                tree = tree.Parent;
            }
            return string.Join(".", list);
        }

        /// <summary>
        /// Set a global used parameter for all used parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SetGlobalUsedParameter<T>(ParameterKey<T> key, T value)
        {
            UsedParameters.Set(key, value);
            foreach (var child in Children)
            {
                child.Value.SetGlobalUsedParameter<T>(key, value);
            }
        }
    }
}