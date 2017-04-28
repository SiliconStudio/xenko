// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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