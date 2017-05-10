// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    internal class CompassSensor : Sensor, ICompassSensor
    {
        public float Heading { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompassSensor"/> class.
        /// </summary>
        public CompassSensor(IInputSource source, string systemName) : base(source, systemName, "Compass")
        {
        }
    }
}