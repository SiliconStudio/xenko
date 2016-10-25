// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Hardware;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

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

        protected AndroidSensorListener listener;
        private string sensorName;
        private Guid sensorId;
        private Sensor sensor;
        private SensorManager sensorManager;
        private Vector3 lastReading;

        public SensorAndroid(SensorType androidType)
        {
            sensorName = $"Android Sensor [{androidType}]";
            sensorId = InputDeviceUtils.DeviceNameToGuid(androidType.ToString());

            listener = new AndroidSensorListener(this);
            sensorManager = (SensorManager)PlatformAndroid.Context.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(androidType);
        }

        public override void Update()
        {
            base.Update();
            HandleSensorChanged(listener.GetValues());
        }

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
            private IList<float> lastValues;
            private readonly List<float> lastQueriedValues = new List<float>();
            private bool updated = false;
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
                // Store reading
                lastValues = e.Values;
                updated = true;
            }

            public IReadOnlyList<float> GetValues()
            {
                if (lastValues == null)
                    return null;
                if (updated)
                {
                    lastQueriedValues.Clear();
                    for (int i = 0; i < lastValues.Count; i++)
                    {
                        lastQueriedValues.Add(lastValues[i]);
                    }
                    updated = false;
                }
                return lastQueriedValues;
            }

            public float GetCurrentValueAsFloat()
            {
                var values = GetValues();
                return values?[0] ?? 0.0f;
            }

            public Vector3 GetCurrentValuesAsVector()
            {
                var values = GetValues();
                return (values != null && values.Count >= 3) ? new Vector3(-values[0], -values[2], -values[1]) : Vector3.Zero;
            }
        }
    }
}

#endif