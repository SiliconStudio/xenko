// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Background;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Add a background to an <see cref="Entity"/>.
    /// </summary>
    [DataContract("BackgroundComponent")]
    [Display("Background", Expand = ExpandRule.Once)]
    [DefaultEntityComponentRenderer(typeof(BackgroundRenderProcessor))]
    [ComponentOrder(9600)]
    public sealed class BackgroundComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Create an empty Background component.
        /// </summary>
        public BackgroundComponent()
        {
            Intensity = 1f;
        }

        /// <summary>
        /// Gets or sets the texture to use as background
        /// </summary>
        /// <userdoc>The reference to the texture to use as background</userdoc>
        [DataMember(10)]
        [Display("Texture")]
        public Texture Texture { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        /// <userdoc>The intensity of the background color</userdoc>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 100.0, 0.01f, 1.0f)]
        public float Intensity { get; set; }
    }
}