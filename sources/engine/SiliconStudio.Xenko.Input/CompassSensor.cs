// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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