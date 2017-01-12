// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class NamedUserAccelerationSensor : NamedSensor, IUserAccelerationSensor
    {
        public Vector3 Acceleration { get; internal set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedUserAccelerationSensor"/> class.
        /// </summary>
        public NamedUserAccelerationSensor(IInputSource source, string systemName) : base(source, systemName, "User Acceleration")
        {
        }
    }
}