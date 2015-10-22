// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// This class represents a sensor of type user acceleration. It measures the acceleration applied by the user (no gravity) onto the device.
    /// </summary>
    public class UserAccelerationSensor : SensorBase
    {
        /// <summary>
        /// Gets the current acceleration applied by the user (in meters/seconds^2).
        /// </summary>
        public Vector3 Acceleration { get; internal set; }

        internal override void ResetData()
        {
            Acceleration = Vector3.Zero;
        }
    }
}