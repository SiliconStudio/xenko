// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// Defines how the background parameters used for this skybox.
    /// </summary>
    [DataContract("SkyboxBackground")]
    public class SkyboxBackground
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxBackground"/> class.
        /// </summary>
        public SkyboxBackground()
        {
            Enabled = true;
            Intensity = 1.0f;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SkyboxBackground"/> is enabled.
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

        // TODO: Add selection for skybox or ambient only
    }
}