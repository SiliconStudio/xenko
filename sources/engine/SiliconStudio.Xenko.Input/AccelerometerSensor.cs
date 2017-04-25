// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Accelerometer. It measures the acceleration forces (including gravity) applying on the device.
    /// </summary>
    public class AccelerometerSensor : SensorBase
    {
        /// <summary>
        /// Gets the current acceleration applied on the device (in meters/seconds^2).
        /// </summary>
        public Vector3 Acceleration { get; internal set; }

        internal override void ResetData()
        {
            Acceleration = Vector3.Zero;
        }
    }
}
