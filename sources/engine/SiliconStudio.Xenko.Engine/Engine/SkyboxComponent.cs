// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Rendering.Skyboxes;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Add a <see cref="Skybox"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("SkyboxComponent")]
    [Display("Skybox", Expand = ExpandRule.Once)]  // More important than lights, as usually the Skybox is associated with a light
    [DefaultEntityComponentRenderer(typeof(SkyboxRenderProcessor))]
    [ComponentOrder(11500)]
    public sealed class SkyboxComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponent"/> class.
        /// </summary>
        public SkyboxComponent() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxComponent"/> class.
        /// </summary>
        /// <param name="skybox">The skybox.</param>
        public SkyboxComponent(Skybox skybox)
        {
            Skybox = skybox;
            Background = SkyboxBackground.Color;
            Intensity = 1.0f;
        }

        /// <summary>
        /// Gets or sets the skybox.
        /// </summary>
        /// <value>
        /// The skybox.
        /// </value>
        /// <userdoc>The skybox to use as input</userdoc>
        [DataMember(20)]
        [DefaultValue(null)]
        public Skybox Skybox { get; set; }

        /// <summary>
        /// Gets or sets the background.
        /// </summary>
        /// <value>The background.</value>
        /// <userdoc>Specify how to display skybox in the background</userdoc>
        [DataMember(30)]
        [DefaultValue(SkyboxBackground.Color)]
        public SkyboxBackground Background { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        /// <userdoc>The light intensity of the skybox</userdoc>
        [DataMember(40)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 100.0, 0.01f, 1.0f)]
        public float Intensity { get; set; }
    }
}