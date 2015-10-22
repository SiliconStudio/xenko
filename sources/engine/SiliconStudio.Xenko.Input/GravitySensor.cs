// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// This class represents a sensor of type Gravity. It measures the gravity force applying on the device.
    /// </summary>
    public class GravitySensor : SensorBase
    {
        /// <summary>
        /// Gets the current gravity applied on the device (in meters/seconds^2).
        /// </summary>
        public Vector3 Vector { get; internal set; }

        internal override void ResetData()
        {
            Vector = Vector3.Zero;
        }
    }
}