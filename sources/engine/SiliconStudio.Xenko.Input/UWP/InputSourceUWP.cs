// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_UWP
using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.System;
using Windows.Devices.Sensors;
using Windows.Gaming.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using SiliconStudio.Core.Mathematics;
using WindowsAccelerometer = Windows.Devices.Sensors.Accelerometer;
using WindowsGyroscope = Windows.Devices.Sensors.Gyrometer;
using WindowsOrientation = Windows.Devices.Sensors.OrientationSensor;
using WindowsCompass = Windows.Devices.Sensors.Compass;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Input source for devices using the universal windows platform
    /// </summary>
    public class InputSourceUWP : InputSourceBase
    {
        private const uint DesiredSensorUpdateIntervalMs = (uint)(1f / InputManager.DesiredSensorUpdateRate * 1000f);

        private Dictionary<Gamepad, GamePadUWP> gamePads = new Dictionary<Gamepad, GamePadUWP>();

        private WindowsAccelerometer windowsAccelerometer;
        private WindowsCompass windowsCompass;
        private WindowsGyroscope windowsGyroscope;
        private WindowsOrientation windowsOrientation;

        private NamedAccelerometerSensor accelerometer;
        private NamedCompassSensor compass;
        private NamedGyroscopeSensor gyroscope;
        private NamedOrientationSensor orientation;
        private NamedGravitySensor gravity;
        private NamedUserAccelerationSensor userAcceleration;

        private PointerUWP pointer;
        private KeyboardUWP keyboard;

        public override void Initialize(InputManager inputManager)
        {
            var windowHandle = inputManager.Game.Window.NativeWindow;
            var uiControl = (FrameworkElement)windowHandle.NativeWindow;

            var mouseCapabilities = new MouseCapabilities();
            if (mouseCapabilities.MousePresent > 0)
            {
                pointer = new PointerUWP(uiControl);
                RegisterDevice(pointer);
            }

            var keyboardCapabilities = new KeyboardCapabilities();
            if (keyboardCapabilities.KeyboardPresent > 0)
            {
                keyboard = new KeyboardUWP(inputManager.Game, uiControl);
                RegisterDevice(keyboard);
            }

            // get sensor default instances
            windowsAccelerometer = WindowsAccelerometer.GetDefault();
            if (windowsAccelerometer != null)
            {
                accelerometer = new NamedAccelerometerSensor("UWP");
                RegisterDevice(accelerometer);
            }
            windowsCompass = WindowsCompass.GetDefault();
            if (windowsCompass != null)
            {
                compass = new NamedCompassSensor("UWP");
                RegisterDevice(compass);
            }
            windowsGyroscope = WindowsGyroscope.GetDefault();
            if (windowsGyroscope != null)
            {
                gyroscope = new NamedGyroscopeSensor("UWP");
                RegisterDevice(gyroscope);
            }
            windowsOrientation = WindowsOrientation.GetDefault();
            if (windowsOrientation != null)
            {
                orientation = new NamedOrientationSensor("UWP");
                RegisterDevice(orientation);
            }

            // Virtual sensors
            if (windowsOrientation != null && windowsAccelerometer != null)
            {
                gravity = new NamedGravitySensor("UWP");
                userAcceleration = new NamedUserAccelerationSensor("UWP");
                RegisterDevice(gravity);
                RegisterDevice(userAcceleration);
            }

            Gamepad.GamepadAdded += GamepadOnGamepadAdded;
            Gamepad.GamepadRemoved += GamepadOnGamepadRemoved;
            
            Scan();
        }

        public override void Scan()
        {
            base.Scan();

            foreach (var gamepad in Gamepad.Gamepads)
            {
                GamepadOnGamepadAdded(this, gamepad);
            }
        }

        private void GamepadOnGamepadRemoved(object sender, Gamepad gamepad)
        {
            GamePadUWP currentGamePad;
            if (!gamePads.TryGetValue(gamepad, out currentGamePad))
                return;

            gamePads.Remove(gamepad);
            UnregisterDevice(currentGamePad);
        }

        private void GamepadOnGamepadAdded(object sender, Gamepad gamepad)
        {
            if (gamePads.ContainsKey(gamepad))
                return;

            GamePadUWP newGamePad = new GamePadUWP(gamepad, Guid.NewGuid());
            gamePads.Add(gamepad, newGamePad);
            RegisterDevice(newGamePad);
        }

        public override void Update()
        {
            base.Update();

            // Enable/disable supported sensors and update enabled sensors
            if (accelerometer != null)
            {
                bool enable = accelerometer.IsEnabled || (userAcceleration?.IsEnabled ?? false) || (gravity?.IsEnabled ?? false);
                bool isEnabled = windowsAccelerometer.ReportInterval != 0;
                if (enable != isEnabled)
                {
                    if (enable)
                        windowsAccelerometer.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsAccelerometer.MinimumReportInterval);
                    else
                        windowsAccelerometer.ReportInterval = 0;
                }
                if (enable)
                {
                    accelerometer.AccelerationInternal = GetAcceleration(windowsAccelerometer);
                }
            }
            if (compass != null)
            {
                bool enable = compass.IsEnabled;
                bool isEnabled = windowsCompass.ReportInterval != 0;
                if (enable != isEnabled)
                {
                    if (enable)
                        windowsCompass.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsCompass.MinimumReportInterval);
                    else
                        windowsCompass.ReportInterval = 0;
                }
                if (enable)
                {
                    compass.HeadingInternal = GetNorth(windowsCompass);
                }
            }
            if (gyroscope != null)
            {
                bool enable = gyroscope.IsEnabled;
                bool isEnabled = windowsGyroscope.ReportInterval != 0;
                if (enable != isEnabled)
                {
                    if (enable)
                        windowsGyroscope.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsGyroscope.MinimumReportInterval);
                    else
                        windowsGyroscope.ReportInterval = 0;
                }
                if (enable)
                {
                    var reading = windowsGyroscope.GetCurrentReading();
                    gyroscope.RotationRateInternal = reading != null ? new Vector3((float)reading.AngularVelocityX, (float)reading.AngularVelocityZ, -(float)reading.AngularVelocityY) : Vector3.Zero;
                }
            }
            if (orientation != null)
            {
                bool enable = orientation.IsEnabled || (userAcceleration?.IsEnabled ?? false) || (gravity?.IsEnabled ?? false);
                bool isEnabled = windowsOrientation.ReportInterval != 0;
                if (enable != isEnabled)
                {
                    if (enable)
                        windowsOrientation.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsOrientation.MinimumReportInterval);
                    else
                        windowsOrientation.ReportInterval = 0;
                }
                if (enable)
                {
                    var quaternion = GetOrientation(windowsOrientation);
                    orientation.FromQuaternion(quaternion);

                    if (userAcceleration.IsEnabled || gravity.IsEnabled)
                    {
                        // calculate the gravity direction
                        var acceleration = GetAcceleration(windowsAccelerometer);
                        var gravityDirection = Vector3.Transform(-Vector3.UnitY, Quaternion.Invert(quaternion));
                        var gravity = InputManager.G * gravityDirection;

                        this.gravity.VectorInternal = gravity;
                        userAcceleration.AccelerationInternal = acceleration - gravity;
                    }
                }
            }
        }

        public override void Pause()
        {
            base.Pause();
            if (windowsAccelerometer != null)
                windowsAccelerometer.ReportInterval = 0;

            if (windowsCompass != null)
                windowsCompass.ReportInterval = 0;

            if (windowsGyroscope != null)
                windowsGyroscope.ReportInterval = 0;

            if (windowsOrientation != null)
                windowsOrientation.ReportInterval = 0;
        }

        private static Vector3 GetAcceleration(WindowsAccelerometer accelerometer)
        {
            var currentReading = accelerometer.GetCurrentReading();
            if (currentReading == null)
                return Vector3.Zero;

            return InputManager.G * new Vector3((float)currentReading.AccelerationX, (float)currentReading.AccelerationZ, -(float)currentReading.AccelerationY);
        }

        private static Quaternion GetOrientation(WindowsOrientation orientation)
        {
            var reading = orientation.GetCurrentReading();
            if (reading == null)
                return Quaternion.Identity;

            var q = reading.Quaternion;
            return new Quaternion(q.X, q.Z, -q.Y, q.W);
        }

        private static float GetNorth(WindowsCompass compass)
        {
            var currentReading = compass.GetCurrentReading();
            if (currentReading == null)
                return 0f;

            return MathUtil.DegreesToRadians((float)(currentReading.HeadingTrueNorth ?? currentReading.HeadingMagneticNorth));
        }
    }
}

#endif