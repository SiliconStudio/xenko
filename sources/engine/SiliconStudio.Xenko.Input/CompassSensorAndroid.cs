// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class CompassSensorAndroid : SensorDeviceBase, ICompassSensor
    {
        public override string DeviceName { get; }
        public override Guid Id { get; }
        public float Heading => heading;
        
        private float heading;
        private InputSourceAndroid source;
        private OrientationSensorAndroid orientationSensor;

        public CompassSensorAndroid(InputSourceAndroid source)
        {
            DeviceName = $"Android Sensor [Compass]";
            Id = InputDeviceUtils.DeviceNameToGuid("Compass");
            this.source = source;
        }

        protected override bool EnableImpl()
        {
            // Depend on orientation sensor
            orientationSensor = (OrientationSensorAndroid)source.TryGetSensor(typeof(OrientationSensorAndroid));
            return orientationSensor != null;
        }
        protected override void DisableImpl()
        {
            orientationSensor = null;
        }

        public override void Update()
        {
            base.Update();
            if (orientationSensor != null)
            {
                // Update heading
                HandleSensorChanged(new List<float> { orientationSensor.YawPitchRollArray[0] + MathUtil.Pi});
            }
        }
    }
}
#endif