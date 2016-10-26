// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class NamedGyroscopeSensor : NamedSensor, IGyroscopeSensor
    {
        public Vector3 RotationRate => RotationRateInternal;
        internal Vector3 RotationRateInternal;
        
        public NamedGyroscopeSensor(string systemName) : base(systemName, "Gyroscope")
        {
        }
    }
}