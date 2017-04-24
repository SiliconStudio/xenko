// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// The base class for a device sensor.
    /// </summary>
    public abstract class SensorBase
    {
        private bool isEnabled;

        internal bool ShouldBeEnabled;
        internal bool ShouldBeDisabled;

        /// <summary>
        /// Gets or sets the sensor enabled state.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to enable a sensor type that is not supported.</exception>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if(isEnabled == value)
                    return;

                if(!IsSupported)
                    throw new InvalidOperationException("The sensor of type '{0}' is not supported on current device".ToFormat(GetType().Name));

                ShouldBeEnabled = value;
                ShouldBeDisabled = !value;

                isEnabled = value;
            }
        }

        /// <summary>
        /// Gets the value indicating if the sensor is available on the current device.
        /// </summary>
        public bool IsSupported { get; internal set; }

        /// <summary>
        /// Reset the current data of the sensor
        /// </summary>
        internal abstract void ResetData();
    }
}
