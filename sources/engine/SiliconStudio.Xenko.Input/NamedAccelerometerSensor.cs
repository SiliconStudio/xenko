// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class NamedAccelerometerSensor : NamedSensor, IAccelerometerSensor
    {
        public Vector3 Acceleration => AccelerationInternal;
        internal Vector3 AccelerationInternal;

        public NamedAccelerometerSensor(string systemName) : base(systemName, "Accelerometer")
        {
        }
    }
}