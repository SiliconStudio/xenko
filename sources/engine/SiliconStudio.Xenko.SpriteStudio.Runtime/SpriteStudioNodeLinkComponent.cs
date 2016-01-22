// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    [DataContract("SpriteStudioNodeLinkComponent")]
    [Display("Sprite Studio Node Link", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(SpriteStudioNodeLinkProcessor))]
    [ComponentOrder(1400)]
    public sealed class SpriteStudioNodeLinkComponent : EntityComponent
    {
        /// <summary>
        /// Gets or sets the model which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The model which contains the hierarchy to use.
        /// </value>
        /// <userdoc>The reference to the target entity to which attach the current entity. If null, parent will be used.</userdoc>
        [Display("Target (Parent if not set)")]
        public SpriteStudioComponent Target { get; set; }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        /// <userdoc>The name of node of the model of the target entity to which attach the current entity.</userdoc>
        public string NodeName { get; set; }
    }
}