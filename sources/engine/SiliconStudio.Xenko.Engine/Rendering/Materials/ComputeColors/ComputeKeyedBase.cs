// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
