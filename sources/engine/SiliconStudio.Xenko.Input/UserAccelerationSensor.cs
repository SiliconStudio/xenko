// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
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
