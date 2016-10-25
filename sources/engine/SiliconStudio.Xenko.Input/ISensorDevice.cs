// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Interface for a sensor device, which can report any amount of float values
    /// </summary>
    public interface ISensorDevice : IInputDevice
    {
        /// <summary>
        /// The sensor's values
        /// </summary>
        IReadOnlyList<float> Values { get; }

        /// <summary>
        /// Raised when the sensor's values have changed
        /// </summary>
        EventHandler<SensorEvent> OnValuesChanged { get; set; }

        /// <summary>
        /// Gets or sets if this sensor is enabled (disabling it will save battery power on mobile devices)
        /// </summary>
        bool IsEnabled { get; set; }
    }
}