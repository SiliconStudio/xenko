// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System.Collections.Generic;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GravitySensorAndroid : SensorAndroid, IGravitySensor
    {
        public Vector3 Vector => vector;
        private Vector3 vector;

        public GravitySensorAndroid() : base(SensorType.Gravity)
        {
        }
        public override void UpdateSensorData(IReadOnlyList<float> newValues)
        {
            vector = this.AsVector();
        }

    }
}
#endif