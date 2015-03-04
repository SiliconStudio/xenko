// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A spot light.
    /// </summary>
    [DataContract("LightSpot")]
    [Display("Spot")]
    public class LightSpot : DirectLightBase
    {
        public LightSpot()
        {
            BeamAngle = 30;
            FieldAngle = 60;
            DecayStart = 20;
        }

        /// <summary>
        /// Gets or sets the decay start.
        /// </summary>
        /// <value>The decay start.</value>
        [DefaultValue(20.0f)]
        public float DecayStart { get; set; }

        /// <summary>Gets or sets the beam angle of the spot light.</summary>
        /// <value>The beam angle of the spot (in degrees between 0 and 90).</value>
        [DefaultValue(30.0f)]
        public float BeamAngle { get; set; }

        /// <summary>Gets or sets the spot field angle of the spot light.</summary>
        /// <value>The spot field angle of the spot (in degrees between 0 and 90).</value>
        [DefaultValue(60.0f)]
        public float FieldAngle { get; set; }
    }
}