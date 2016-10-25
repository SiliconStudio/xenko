// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GyroscopeSensorAndroid : SensorAndroid, IGyroscopeSensor
    {
        public Vector3 RotationRate => rotationRate;
        private Vector3 rotationRate;

        public GyroscopeSensorAndroid() : base(SensorType.Gyroscope)
        {
        }

        public override void Update()
        {
            base.Update();
            rotationRate = -listener.GetCurrentValuesAsVector();
        }
    }
}

#endif