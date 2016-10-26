// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a sensor that implements Enabled/Disable and provides a name/guid set from constructor
    /// </summary>
    public class NamedSensor : ISensorDevice
    {
        public NamedSensor(string systemName, string sensorName)
        {
            DeviceName = $"{systemName} {sensorName} Sensor";
            Id = InputDeviceUtils.DeviceNameToGuid(systemName + sensorName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public string DeviceName { get; }

        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <inheritdoc />
        public bool IsEnabled { get; set; }
        
        /// <inheritdoc />
        public virtual void Update(List<InputEvent> inputEvents)
        {
        }
    }
}