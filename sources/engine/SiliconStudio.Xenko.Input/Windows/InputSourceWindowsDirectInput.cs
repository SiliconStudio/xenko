// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SharpDX.DirectInput;
using SiliconStudio.Xenko.Native.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for various game controllers on windows
    /// </summary>
    public class InputSourceWindowsDirectInput : InputSourceBase
    {
        private InputManager inputManager;
        private readonly HashSet<Guid> devicesToRemove = new HashSet<Guid>();
        private DirectInput directInput;

        public override void Dispose()
        {
            // Dispose all the gamepads
            foreach (var pair in InputDevices)
            {
                pair.Value.Dispose();
            }
            
            // Unregisters all devices
            base.Dispose();

            // Dispose DirectInput
            directInput.Dispose();
        }

        public override void Initialize(InputManager inputManager)
        {
            this.inputManager = inputManager;
            directInput = new DirectInput();
            Scan();
        }

        public override void Update()
        {
            // Process device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = InputDevices[deviceIdToRemove] as GameControllerDirectInput;
                UnregisterDevice(gamePad);

                if (gamePad.IsConnected)
                    gamePad.Dispose();
            }
            devicesToRemove.Clear();
        }

        /// <summary>
        /// Scans for new devices
        /// </summary>
        public override void Scan()
        {
            var connectedDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            foreach (var device in connectedDevices)
            {
                if (!InputDevices.ContainsKey(device.InstanceGuid))
                {
                    OpenDevice(device);
                }
            }
        }

        /// <summary>
        /// Opens a new game controller or gamepad
        /// </summary>
        /// <param name="deviceInstance">The device instance</param>
        public void OpenDevice(DeviceInstance deviceInstance)
        {
            // Ignore XInput devices since they are handled by XInput
            if (XInputChecker.IsXInputDevice(ref deviceInstance.ProductGuid))
                return;

            if (InputDevices.ContainsKey(deviceInstance.InstanceGuid))
                throw new InvalidOperationException($"DirectInput GameController already opened {deviceInstance.InstanceGuid}/{deviceInstance.InstanceName}");

            // Find gamepad layout
            var layout = GamePadLayouts.FindLayout(this, deviceInstance.ProductName, deviceInstance.ProductGuid);

            var newGamepad = (layout != null) ? 
                new GamePadDirectInput(inputManager, directInput, deviceInstance, layout) : 
                new GameControllerDirectInput(directInput, deviceInstance);

            newGamepad.Disconnected += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(newGamepad.Id);
            };
            RegisterDevice(newGamepad);
        }
    }
}

#endif