// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Interface for a sensor device, use more specific interfaces to retrieve sensor data
    /// </summary>
    public interface ISensorDevice : IInputDevice
    {
        /// <summary>
        /// Gets or sets if this sensor is enabled
        /// </summary>
        /// <remarks>Sensors are disabled by default</remarks>
        /// <remarks>Disabling unused sensors will save battery power on mobile devices</remarks>
        bool IsEnabled { get; set; }
    }
}