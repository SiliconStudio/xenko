// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Base interface for all computer color nodes.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class ComputeKeyedBase : ComputeNode
    {
        /// <summary>
        /// Gets or sets a custom key associated to this node.
        /// </summary>
        /// <value>The key.</value>
        [DataMemberIgnore]
        [DefaultValue(null)]
        public ParameterKey Key { get; set; }

        /// <summary>
        /// Gets or sets the used key.
        /// </summary>
        /// <value>The used key.</value>
        [DataMemberIgnore]
        public ParameterKey UsedKey { get; set; }
    }
}