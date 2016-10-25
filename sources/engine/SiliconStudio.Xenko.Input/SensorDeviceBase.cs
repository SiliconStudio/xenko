// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for sensor devices
    /// </summary>
    public abstract class SensorDeviceBase : ISensorDevice
    {
        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public int Priority { get; set; }
        public IReadOnlyList<float> Values => values;
        public EventHandler<SensorEvent> OnValuesChanged { get; set; }

        public bool IsEnabled
        {
            set
            {
                if (value)
                    Enable();
                else
                    Disable();
            }
            get { return isEnabled; }
        }

        protected bool isEnabled = false;
        private readonly List<SensorEvent> sensorEvents = new List<SensorEvent>();
        private List<float> values = new List<float>();

        public virtual void Dispose()
        {
        }

        public virtual void Update()
        {
            foreach (var evt in sensorEvents)
            {
                values = evt.Values.ToList();
                OnValuesChanged?.Invoke(this, evt);
            }
            sensorEvents.Clear();
        }

        /// <summary>
        /// Tries to enabled the sensor
        /// </summary>
        /// <returns><c>true</c> if the sensor was succcessfully enabled</returns>
        public bool Enable()
        {
            if (!isEnabled)
            {
                if (!EnableImpl())
                    return false;
                isEnabled = true;
            }
            return true;
        }

        /// <summary>
        /// Disables the sensor
        /// </summary>
        public void Disable()
        {
            if (isEnabled)
            {
                DisableImpl();
            }
            isEnabled = false;
        }

        /// <summary>
        /// Tries to enabled the sensor
        /// </summary>
        /// <returns><c>true</c> if the sensor was succcessfully enabled</returns>
        protected abstract bool EnableImpl();

        /// <summary>
        /// Disables the sensor
        /// </summary>
        protected abstract void DisableImpl();

        internal void HandleSensorChanged(IReadOnlyList<float> newValues)
        {
            sensorEvents.Add(new SensorEvent(newValues));
        }
    }
}