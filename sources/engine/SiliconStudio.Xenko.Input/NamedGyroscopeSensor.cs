// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class NamedGyroscopeSensor : NamedSensor, IGyroscopeSensor
    {
        public Vector3 RotationRate { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedGyroscopeSensor"/> class.
        /// </summary>
        public NamedGyroscopeSensor(IInputSource source, string systemName) : base(source, systemName, "Gyroscope")
        {
        }
    }
}