// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Hardware;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for a sensor that listens to an android sensor
    /// </summary>
    public abstract class SensorAndroid : SensorDeviceBase
    {
        private const int SensorDesiredUpdateDelay = (int)(1/InputManager.DesiredSensorUpdateRate*1000f*1000.0f);
        
        public override string DeviceName => sensorName;
        public override Guid Id => sensorId;
        
        private string sensorName;
        private Guid sensorId;
        private AndroidSensorListener listener;
        private Sensor sensor;
        private SensorManager sensorManager;


        public SensorAndroid(SensorType androidType)
        {
            sensorName = $"Android Sensor [{androidType}]";
            sensorId = InputDeviceUtils.DeviceNameToGuid(androidType.ToString());

            listener = new AndroidSensorListener(this);
            sensorManager = (SensorManager)PlatformAndroid.Context.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(androidType);
        }

        /// <summary>
        /// Called when the sensor is updated
        /// </summary>
        /// <param name="newValues">Data received from the sensor</param>
        public abstract void UpdateSensorData(IReadOnlyList<float> newValues);

        public override void Dispose()
        {
            base.Dispose();
            DisableImpl();
        }

        protected override bool EnableImpl()
        {
            return sensorManager.RegisterListener(listener, sensor, (SensorDelay)SensorDesiredUpdateDelay);
        }

        protected override void DisableImpl()
        {
            sensorManager.UnregisterListener(listener);
        }
        
        public class AndroidSensorListener : Java.Lang.Object, ISensorEventListener
        {
            private SensorAndroid sensor;
            public AndroidSensorListener(SensorAndroid sensor)
            {
                this.sensor = sensor;
            }

            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
            }
            public virtual void OnSensorChanged(Android.Hardware.SensorEvent e)
            {
                var list = e.Values.ToList();
                sensor.HandleSensorChanged(list);
                sensor.UpdateSensorData(list);
            }
        }
    }
}
#endif