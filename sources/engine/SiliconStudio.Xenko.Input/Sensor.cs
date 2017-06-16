// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a sensor that implements Enabled/Disable and provides a name/guid set from constructor
    /// </summary>
    internal class Sensor : ISensorDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sensor"/> class.
        /// </summary>
        protected Sensor(IInputSource source, string systemName, string sensorName)
        {
            Source = source;
            Name = $"{systemName} {sensorName} Sensor";
            Id = InputDeviceUtils.DeviceNameToGuid(systemName + sensorName);
        }

        public string Name { get; }

        public Guid Id { get; }

        public int Priority { get; set; }

        public IInputSource Source { get; }

        public bool IsEnabled { get; set; }

        public void Update(List<InputEvent> inputEvents)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}