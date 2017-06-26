// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class UserAccelerationSensor : Sensor, IUserAccelerationSensor
    {
        public Vector3 Acceleration { get; internal set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAccelerationSensor"/> class.
        /// </summary>
        public UserAccelerationSensor(IInputSource source, string systemName) : base(source, systemName, "User Acceleration")
        {
        }
    }
}