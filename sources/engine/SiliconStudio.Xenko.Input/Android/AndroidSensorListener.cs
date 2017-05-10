// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Listener for android sensors
    /// </summary>
    internal class AndroidSensorListener : Java.Lang.Object, ISensorEventListener
    {
        private const int SensorDesiredUpdateDelay = (int)(1/InputManager.DesiredSensorUpdateRate * 1000f * 1000.0f);

        private readonly List<float> lastQueriedValues = new List<float>();
        private IList<float> lastValues;
        private bool updated;
        private Android.Hardware.Sensor sensor;
        private SensorManager sensorManager;

        public AndroidSensorListener(SensorType sensorType)
        {
            sensorManager = (SensorManager)PlatformAndroid.Context.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(sensorType);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disable();
        }

        public bool Enabled { get; private set; } = false;

        public bool Enable()
        {
            if (!Enabled)
            {
                if (!sensorManager.RegisterListener(this, sensor, (SensorDelay)SensorDesiredUpdateDelay))
                    return false;
                Enabled = true;
            }
            return true;
        }

        public void Disable()
        {
            if (Enabled)
            {
                sensorManager.UnregisterListener(this);
                Enabled = false;
            }
        }

        public void OnAccuracyChanged(Android.Hardware.Sensor sensor, SensorStatus accuracy)
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

#endif