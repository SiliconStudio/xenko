// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class GravitySensor : Sensor, IGravitySensor
    {
        public Vector3 Vector { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GravitySensor"/> class.
        /// </summary>
        public GravitySensor(IInputSource source, string systemName) : base(source, systemName, "Gravity")
        {
        }
    }
}