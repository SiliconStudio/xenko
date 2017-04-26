// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class GyroscopeSensor : Sensor, IGyroscopeSensor
    {
        public Vector3 RotationRate { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GyroscopeSensor"/> class.
        /// </summary>
        public GyroscopeSensor(IInputSource source, string systemName) : base(source, systemName, "Gyroscope")
        {
        }
    }
}