// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX.XInput;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for XInput gamepads
    /// </summary>
    public class InputSourceWindowsXInput : InputSourceBase
    {
        private const int XInputGamePadCount = 4;

        // Always monitored gamepads
        private Controller[] controllers;
        private Guid[] controllerIds;
        private GamePadXInput[] devices;

        private readonly List<int> devicesToRemove = new List<int>();
        
        public override void Dispose()
        {
            base.Dispose();

            // Dispose all the gamepads
            foreach (var gamePad in devices)
            {
                gamePad?.Dispose();
            }
        }

        public override void Initialize(InputManager inputManager)
        {
            try
            {
                Controller.SetReporting(true);

                controllers = new Controller[XInputGamePadCount];
                controllerIds = new Guid[XInputGamePadCount];
                devices = new GamePadXInput[XInputGamePadCount];

                // Prebuild fake GUID
                for (int i = 0; i < XInputGamePadCount; i++)
                {
                    controllerIds[i] = new Guid(i, 11, 22, 33, 0, 0, 0, 0, 0, 0, 0);
                    controllers[i] = new Controller((UserIndex)i);
                }
                Scan();
            }
            catch (Exception ex)
            {
                InputManager.Logger.Warning("XInput dll was not found on the computer. GamePad detection will not fully work for the current game instance. " +
                                     "To fix the problem, please install or repair DirectX installation. [Exception details: {0}]", ex.Message);
            }
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext is GameContext<Control> || 
                gameContext.ContextType == AppContextType.DesktopOpenTK;
        }

        public override void Update()
        {
            // Notify event listeners of device removals
            foreach (var deviceIdToRemove in devicesToRemove)
            {
                var gamePad = devices[deviceIdToRemove];
                UnregisterDevice(gamePad);
                devices[deviceIdToRemove] = null;

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
            // TODO: put in try/catch in case XInput.dll can not be found and don't retry
            for (int i = 0; i < XInputGamePadCount; i++)
            {
                if (devices[i] == null)
                {
                    // Should register controller
                    if (controllers[i].IsConnected)
                    {
                        OpenDevice(i);
                    }
                }
            }
        }

        /// <summary>
        /// Opens a new gamepad
        /// </summary>
        /// <param name="instance">The gamepad</param>
        public void OpenDevice(int index)
        {
            if (index < 0 || index >= XInputGamePadCount)
                throw new IndexOutOfRangeException($"Invalid XInput device index {index}");
            if (devices[index] != null)
                throw new InvalidOperationException($"XInput device already opened {index}");

            var newGamepad = new GamePadXInput(controllers[index], controllerIds[index], index);
            newGamepad.Disconnected += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(index);
            };
            devices[index] = newGamepad;
            RegisterDevice(newGamepad);
        }
    }
}

#endif