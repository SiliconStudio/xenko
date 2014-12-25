// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// Base interface for all computer color nodes.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class MaterialKeyedComputeColor : MaterialComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialComputeColor"/> class.
        /// </summary>
        protected MaterialKeyedComputeColor()
        {
        }

        /// <summary>
        /// Gets or sets a custom key associated to this node.
        /// </summary>
        /// <value>The key.</value>
        [DataMember(100)]
        [DefaultValue(null)]
        public ParameterKey Key { get; set; }
    }
}