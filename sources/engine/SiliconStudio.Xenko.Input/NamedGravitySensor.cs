// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class NamedGravitySensor : NamedSensor, IGravitySensor
    {
        public Vector3 Vector => VectorInternal;
        internal Vector3 VectorInternal;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedGravitySensor"/> class.
        /// </summary>
        public NamedGravitySensor(string systemName) : base(systemName, "Gravity")
        {
        }
    }
}