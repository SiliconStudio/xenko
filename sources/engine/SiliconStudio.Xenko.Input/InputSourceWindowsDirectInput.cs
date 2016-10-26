// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SharpDX.DirectInput;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Native.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for various game controllers on windows
    /// </summary>
    public class InputSourceWindowsDirectInput : InputSourceBase
    {
        private DirectInput directInput;

        // TODO: Merge with InputSourceBase maybe
        private Dictionary<Guid, GamePadDirectInput> registeredDevices = new Dictionary<Guid, GamePadDirectInput>();
        private HashSet<Guid> devicesToRemove = new HashSet<Guid>();

        public override void Initialize(InputManager inputManager)
        {
            directInput = new DirectInput();
            Scan();
        }

        public override bool IsEnabled(GameContext gameContext)
        { 
            return gameContext is GameContext<System.Windows.Forms.Control>;
        }

        public override void Update()
        {
            // Notify event listeners of device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = registeredDevices[deviceIdToRemove];
                UnregisterDevice(gamePad);
                registeredDevices.Remove(deviceIdToRemove);

                if (gamePad.Connected)
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
                if (!registeredDevices.ContainsKey(device.InstanceGuid))
                {
                    OpenDevice(device);
                }
            }
        }

        /// <summary>
        /// Opens a new gamepad
        /// </summary>
        /// <param name="instance">The gamepad</param>
        public void OpenDevice(DeviceInstance instance)
        {
            // Ignore XInput devices since they are handled by XInput
            if (XInputChecker.IsXInputDevice(ref instance.ProductGuid))
                return;

            if (registeredDevices.ContainsKey(instance.InstanceGuid))
                throw new InvalidOperationException($"DirectInput GamePad already opened {instance.InstanceGuid}/{instance.InstanceName}");

            var newGamepad = new GamePadDirectInput(directInput, instance);
            newGamepad.OnDisconnect += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(newGamepad.Id);
            };
            registeredDevices.Add(newGamepad.Id, newGamepad);
            RegisterDevice(newGamepad);
        }

        public override void Dispose()
        {
            base.Dispose();

            // Dispose all the gamepads
            foreach (var pair in registeredDevices)
            {
                pair.Value.Dispose();
            }
            registeredDevices.Clear();

            // Dispose DirectInput
            directInput.Dispose();
        }
    }
}

#endif