// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// Defines how the lighting parameters used for this skybox.
    /// </summary>
    [DataContract("SkyboxLighting")]
    public class SkyboxLighting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxLighting"/> class.
        /// </summary>
        public SkyboxLighting()
        {
            Enabled = true;
            Intensity = 1.0f;
            Rotation = 0.0f;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SkyboxLighting"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 100.0, 0.01f, 1.0f)]
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        [DefaultValue(0.0f)]
        [DataMemberRange(0.0, 360.0, 1.0f, 10.0f)]
        public float Rotation { get; set; }
    }
}