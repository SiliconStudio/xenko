// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Gravity. It measures the gravity force applying on the device.
    /// </summary>
    public interface IGravitySensor : ISensorDevice
    {
        /// <summary>
        /// Gets the current gravity applied on the device (in meters/seconds^2).
        /// </summary>
        Vector3 Vector { get; }
    }
}