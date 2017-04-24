// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type compass. It measures the angle between the device and the north.
    /// </summary>
    public class CompassSensor : SensorBase
    {
        /// <summary>
        /// Gets the value of north heading, that is the angle (in radian) between the top of the device and north.
        /// </summary>
        public float Heading { get; internal set; }

        internal override void ResetData()
        {
            Heading = 0;
        }
    }
}
