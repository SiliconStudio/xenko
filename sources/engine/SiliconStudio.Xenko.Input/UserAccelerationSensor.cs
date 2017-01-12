// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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