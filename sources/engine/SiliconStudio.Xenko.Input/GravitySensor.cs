// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Gravity. It measures the gravity force applying on the device.
    /// </summary>
    public class GravitySensor : SensorBase
    {
        /// <summary>
        /// Gets the current gravity applied on the device (in meters/seconds^2).
        /// </summary>
        public Vector3 Vector { get; internal set; }

        internal override void ResetData()
        {
            Vector = Vector3.Zero;
        }
    }
}
