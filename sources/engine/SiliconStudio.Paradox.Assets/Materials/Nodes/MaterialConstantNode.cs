// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    /// <summary>
    /// Base class for a constant value for <see cref="IMaterialNode"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MaterialConstantNode<T> : MaterialNodeBase, IMaterialValueNode
    {
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialConstantNode{T}"/> class.
        /// </summary>
        protected MaterialConstantNode()
        {
            AutoAssignKey = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialConstantNode{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected MaterialConstantNode(T value) : this()
        {
            Value = value;
        }

        /// <summary>
        /// The property to access the internal value
        /// </summary>
        /// <userdoc>
        /// The default value.
        /// </userdoc>
        [DataMember(20)]
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;
                }
            }
        }

        /// <summary>
        /// A flag stating if the paramater key is automatically assigned.
        /// </summary>
        /// <userdoc>
        /// If not checked, you can define the key to access the value at runtime for dynamic changes.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(true)]
        public bool AutoAssignKey { get; set; }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        /// <userdoc>
        /// The key to access the value at runtime. AutoAssignKey should be checked.
        /// </userdoc>
        [DataMember(40)]
        [DefaultValue(null)]
        public ParameterKey<T> Key;
    }
}
