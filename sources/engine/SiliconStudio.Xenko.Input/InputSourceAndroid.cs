// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Input
{
    public class InputSourceAndroid : InputSourceBase
    {
        private AndroidXenkoGameView uiControl;
        
        private KeyboardAndroid keyboard;
        private PointerAndroid pointer;

        private AndroidSensorListener accelerometerListener;
        private AndroidSensorListener gyroscopeListener;
        private AndroidSensorListener linearAccelerationListener;
        private AndroidSensorListener orientationListener;
        private AndroidSensorListener gravityListener;
        
        private NamedAccelerometerSensor accelerometerSensor;
        private NamedUserAccelerationSensor userAccelerationSensor;
        private NamedGyroscopeSensor gyroscopeSensor;
        private NamedOrientationSensor orientationSensor;
        private NamedGravitySensor gravitySensor;
        private NamedCompassSensor compassSensor;

        public override void Initialize(InputManager inputManager)
        {
            var context = inputManager.Game.Context as GameContextAndroid;
            uiControl = context.Control;

            // Create android pointer and keyboard
            keyboard = new KeyboardAndroid(uiControl);
            pointer = new PointerAndroid(uiControl);
            RegisterDevice(keyboard);
            RegisterDevice(pointer);

            // Create android sensors
            if ((accelerometerListener = TryGetSensorListener(SensorType.Accelerometer)) != null)
            {
                accelerometerSensor = new NamedAccelerometerSensor("Android");
                RegisterDevice(accelerometerSensor);
            }
            if ((linearAccelerationListener = TryGetSensorListener(SensorType.LinearAcceleration)) != null)
            {
                userAccelerationSensor = new NamedUserAccelerationSensor("Android");
                RegisterDevice(userAccelerationSensor);
            }
            if ((gyroscopeListener = TryGetSensorListener(SensorType.Gyroscope)) != null)
            {
                gyroscopeSensor = new NamedGyroscopeSensor("Android");
                RegisterDevice(gyroscopeSensor);
            }
            if ((gravityListener = TryGetSensorListener(SensorType.Gravity)) != null)
            {
                gravitySensor = new NamedGravitySensor("Android");
                RegisterDevice(gravitySensor);
            }
            if ((orientationListener = TryGetSensorListener(SensorType.RotationVector)) != null)
            {
                orientationSensor = new NamedOrientationSensor("Android");
                compassSensor = new NamedCompassSensor("Android");
                RegisterDevice(orientationSensor);
                RegisterDevice(compassSensor);
            }
        }

        public void UpdateSensorPair<TSensor>(AndroidSensorListener listener, TSensor sensor, Action<TSensor, AndroidSensorListener> updater) where TSensor : NamedSensor
        {
            if (listener != null)
            {
                bool enable = sensor.IsEnabled;
                if (enable != listener.Enabled)
                {
                    if (enable)
                        listener.Enable();
                    else
                        listener.Disable();
                }
                if (enable)
                {
                    updater.Invoke(sensor, listener);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            float[] quaternionArray = new float[4];
            float[] rotationMatrixArray = new float[9];
            float[] YawPitchRollArray = new float[3];

            // Update sensors
            // Enable/disable supported sensors and update enabled sensors
            UpdateSensorPair(accelerometerListener, accelerometerSensor, 
                (sensor, listener) => sensor.AccelerationInternal = listener.GetCurrentValuesAsVector());
            UpdateSensorPair(gyroscopeListener, gyroscopeSensor, 
                (sensor, listener) => sensor.RotationRateInternal = -listener.GetCurrentValuesAsVector());
            UpdateSensorPair(linearAccelerationListener, userAccelerationSensor, 
                (sensor, listener) => sensor.AccelerationInternal = listener.GetCurrentValuesAsVector());
            UpdateSensorPair(gravityListener, gravitySensor, 
                (sensor, listener) => sensor.VectorInternal = listener.GetCurrentValuesAsVector());

            // Enabled/Disable/Update Orientation
            if (orientationListener != null)
            {
                bool enable = orientationSensor.IsEnabled || compassSensor.IsEnabled; // Orientation is used for compass as well
                if (enable != orientationListener.Enabled)
                {
                    if (enable)
                        orientationListener.Enable();
                    else
                        orientationListener.Disable();
                }
                if (enable)
                {
                    // Update orientation
                    base.Update();
                    float[] rotationVector = orientationListener.GetValues()?.ToArray() ?? new float[] {0.0f, 0.0f, 0.0f};
                    if (rotationVector.Length < 3)
                        return;
                    SensorManager.GetQuaternionFromVector(quaternionArray, rotationVector);
                    SensorManager.GetRotationMatrixFromVector(rotationMatrixArray, rotationVector);
                    SensorManager.GetOrientation(rotationMatrixArray, YawPitchRollArray);

                    var quaternion = Quaternion.Identity;
                    quaternion.W = +quaternionArray[0];
                    quaternion.X = +quaternionArray[1];
                    quaternion.Y = +quaternionArray[3];
                    quaternion.Z = -quaternionArray[2];
                    quaternion = Quaternion.RotationY(MathUtil.Pi) * quaternion; // align the orientation with north.
                    orientationSensor.FromQuaternion(quaternion);

                    // Update compass
                    compassSensor.HeadingInternal = YawPitchRollArray[0] + MathUtil.Pi;
                }
            }
        }

        public override void Pause()
        {
            // Disable all sensors when application is paused
            accelerometerListener?.Disable();
            gyroscopeListener?.Disable();
            linearAccelerationListener?.Disable();
            orientationListener?.Disable();
            gravityListener?.Disable();
        }

        public override void Resume()
        {
            base.Resume();
            // Automatic resume when update is called
        }

        /// <summary>
        /// Tries to create a sensor listener if it is supported
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AndroidSensorListener TryGetSensorListener(SensorType type)
        {
            var listener = new AndroidSensorListener(type);
            if (!listener.Enable())
            {
                listener.Dispose();
                return null;
            }
            listener.Disable();
            return listener;
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext.ContextType == AppContextType.Android;
        }
    }
}

#endif