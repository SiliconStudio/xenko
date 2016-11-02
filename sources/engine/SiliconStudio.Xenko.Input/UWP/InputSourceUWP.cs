// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_UWP
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.System;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Input source for devices using the universal windows platform
    /// </summary>
    public class InputSourceUWP : InputSourceBase
    {
        private const uint DesiredSensorUpdateIntervalMs = (uint)(1f/InputManager.DesiredSensorUpdateRate*1000f);

        // mapping between WinRT keys and toolkit keys
        private static readonly MouseCapabilities mouseCapabilities = new MouseCapabilities();

        private Accelerometer windowsAccelerometer;
        private Compass windowsCompass;
        private Gyrometer windowsGyroscope;
        private OrientationSensor windowsOrientation;

        private PointerUWP pointer;
        private KeyboardUWP keyboard;

        public override void Initialize(InputManager inputManager)
        {
            var mouseCapabilities = new MouseCapabilities();
            var windowHandle = inputManager.Game.Window.NativeWindow;
            var frameworkElement = (FrameworkElement)windowHandle.NativeWindow;
            if (mouseCapabilities.MousePresent > 0)
            {
                pointer = new PointerUWP(frameworkElement);
                RegisterDevice(pointer);
            }

            Scan();
        }
    }
}

#endif