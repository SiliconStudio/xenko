// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a sensor that implements Enabled/Disable and provides a name/guid set from constructor
    /// </summary>
    internal class NamedSensor : ISensorDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedSensor"/> class.
        /// </summary>
        public NamedSensor(string systemName, string sensorName)
        {
            Name = $"{systemName} {sensorName} Sensor";
            Id = InputDeviceUtils.DeviceNameToGuid(systemName + sensorName);
        }

        public void Dispose()
        {
        }

        public string Name { get; }
        public Guid Id { get; }
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }

        public virtual void Update(List<InputEvent> inputEvents)
        {
        }
    }
}