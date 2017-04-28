// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Accelerometer. It measures the acceleration forces (including gravity) applying on the device.
    /// </summary>
    public interface IAccelerometerSensor : ISensorDevice
    {
        /// <summary>
        /// Gets the current acceleration applied on the device (in meters/seconds^2).
        /// </summary>
        Vector3 Acceleration { get; }
    }
}