// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Rendering.Skyboxes;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add a <see cref="Skybox"/> to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("SkyboxComponent")]
    [Display(130, "Skybox")]  // More important than lights, as usually the Skybox is associated with a light
    [DefaultEntityComponentRenderer(typeof(SkyboxComponentRenderer), -100)]
    [DefaultEntityComponentProcessor(typeof(SkyboxProcessor))]
    public sealed class SkyboxComponent : EntityComponent
    {
        public static PropertyKey<SkyboxComponent> Key = new PropertyKey<SkyboxComponent>("Key", typeof(SkyboxComponent));

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
        [DataMember(20)]
        [DefaultValue(null)]
        public Skybox Skybox { get; set; }

        /// <summary>
        /// Gets or sets the background.
        /// </summary>
        /// <value>The background.</value>
        [DataMember(30)]
        [DefaultValue(SkyboxBackground.Color)]
        public SkyboxBackground Background { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DataMember(40)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 100.0, 0.01f, 1.0f)]
        public float Intensity { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}