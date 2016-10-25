// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


#if SILICONSTUDIO_PLATFORM_ANDROID

using System.Collections.Generic;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class UserAccelerationSensorAndroid : SensorAndroid, IUserAccelerationSensor
    {
        public Vector3 Acceleration => acceleration;
        private Vector3 acceleration;

        public UserAccelerationSensorAndroid() : base(SensorType.LinearAcceleration)
        {
        }

        public override void Update()
        {
            base.Update();
            acceleration = listener.GetCurrentValuesAsVector();
        }
    }
}
#endif