// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpDX.XInput;
using SiliconStudio.Core.Mathematics;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

namespace SiliconStudio.Xenko.Input
{
    public class XBoxLayoutInfo
    {
        public static readonly List<GamePadButtonInfo> Buttons = new List<GamePadButtonInfo>
        {
            new GamePadButtonInfo { Index = 0, Name = "Start" },
            new GamePadButtonInfo { Index = 1, Name = "Back" },
            new GamePadButtonInfo { Index = 2, Name = "Left Thumb" },
            new GamePadButtonInfo { Index = 3, Name = "Right Thumb" },
            new GamePadButtonInfo { Index = 4, Name = "Left Shoulder" },
            new GamePadButtonInfo { Index = 5, Name = "Right Shoulder" },
            new GamePadButtonInfo { Index = 6, Name = "A" },
            new GamePadButtonInfo { Index = 7, Name = "B" },
            new GamePadButtonInfo { Index = 8, Name = "X" },
            new GamePadButtonInfo { Index = 9, Name = "Y" },
        };

        public static readonly List<GamePadAxisInfo> Axes = new List<GamePadAxisInfo>
        {
            new GamePadAxisInfo { Index = 0, Name = "Left Stick X" },
            new GamePadAxisInfo { Index = 1, Name = "Left Stick Y" },
            new GamePadAxisInfo { Index = 2, Name = "Right Stick X" },
            new GamePadAxisInfo { Index = 3, Name = "Right Stick Y" },
            new GamePadAxisInfo { Index = 4, Name = "Left Trigger" },
            new GamePadAxisInfo { Index = 5, Name = "Right Trigger" },
        };

        public static readonly List<GamePadPovControllerInfo> PovControllers = new List<GamePadPovControllerInfo>
        {
            new GamePadPovControllerInfo { Index = 0, Name = "Pad" },
        };
    }

    public class GamePadXInput : GamePadDeviceBase
    {
        public override string DeviceName => $"XInput Controller {index}";
        public override Guid Id => id;
        public override IReadOnlyCollection<GamePadButtonInfo> ButtonInfos => XBoxLayoutInfo.Buttons;
        public override IReadOnlyCollection<GamePadAxisInfo> AxisInfos => XBoxLayoutInfo.Axes;
        public override IReadOnlyCollection<GamePadPovControllerInfo> PovControllerInfos => XBoxLayoutInfo.PovControllers;

        private Controller controller;
        private Guid id;
        private int index;
        private State state;

        public GamePadXInput(Controller controller, Guid id, int index)
        {
            this.controller = controller;
            this.id = id;
            this.index = index;

            InitializeButtonStates();
        }

        public override void Update()
        {
            if (controller.GetState(out state))
            {
                // Process DPad as point of view controller
                GamePadPadDirection padDir = ConvertPadDirection(state.Gamepad.Buttons);
                float pov = GamePadConversions.PadDirectionToPovController(padDir);
                HandlePovController(0, pov);

                // Process buttons
                for (int i = 0; i < ButtonInfos.Count; i++)
                {
                    int mask = 1 << (i + 4);
                    bool buttonState = ((int)state.Gamepad.Buttons & mask) != 0;
                    HandleButton(i, buttonState);
                }

                // Proces Axes
                HandleAxis(0, ClampDeadZone(state.Gamepad.LeftThumbX/32768.0f, Gamepad.LeftThumbDeadZone/32768.0f));
                HandleAxis(1, ClampDeadZone(state.Gamepad.LeftThumbY/32768.0f, Gamepad.LeftThumbDeadZone/32768.0f));

                HandleAxis(2, ClampDeadZone(state.Gamepad.RightThumbX/32768.0f, Gamepad.RightThumbDeadZone/32768.0f));
                HandleAxis(3, ClampDeadZone(state.Gamepad.RightThumbY/32768.0f, Gamepad.RightThumbDeadZone/32768.0f));

                HandleAxis(4, state.Gamepad.LeftTrigger/255.0f);
                HandleAxis(5, state.Gamepad.RightTrigger/255.0f);
            }

            if (!controller.IsConnected)
            {
                OnDisconnect?.Invoke(this, null);
                Dispose();
            }
        }
        
        public GamePadPadDirection ConvertPadDirection(GamepadButtonFlags buttons)
        {
            GamePadPadDirection ret = 0;
            if ((buttons & GamepadButtonFlags.DPadLeft) != 0)
                ret |= GamePadPadDirection.PadLeft;
            if ((buttons & GamepadButtonFlags.DPadUp) != 0)
                ret |= GamePadPadDirection.PadUp;
            if ((buttons & GamepadButtonFlags.DPadDown) != 0)
                ret |= GamePadPadDirection.PadDown;
            if ((buttons & GamepadButtonFlags.DPadRight) != 0)
                ret |= GamePadPadDirection.PadRight;
            return ret;
        }

        public void SetVibration(float leftMotor, float rightMotor)
        {
            leftMotor = MathUtil.Clamp(leftMotor, 0.0f, 1.0f);
            rightMotor = MathUtil.Clamp(rightMotor, 0.0f, 1.0f);
            var vibration = new Vibration
            {
                LeftMotorSpeed = (ushort)(leftMotor*65535.0f),
                RightMotorSpeed = (ushort)(rightMotor*65535.0f)
            };
            controller.SetVibration(vibration);
        }
    }

    public class InputSourceWindowsXInput : InputSourceBase
    {
        private const int XInputGamePadCount = 4;

        // Always monitored gamepads
        private Controller[] controllers;
        private Guid[] controllerIds;
        private GamePadXInput[] devices;

        private List<int> devicesToRemove = new List<int>();

        public override void Initialize(InputManager inputManager)
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

            // Scan for new devices
            // TODO: move to hardware change detection event
            ScanDevices();
        }

        /// <summary>
        /// Scans for new devices
        /// </summary>
        public void ScanDevices()
        {
            // TODO: put in try/catch in case XInput.dll can not be found and don't retry
            for (int i = 0; i < XInputGamePadCount; i++)
            {
                if (devices[i] == null)
                {
                    // Should register controller
                    if (controllers[i].IsConnected)
                    {
                        OpenGamePad(i);
                    }
                }
            }
        }

        /// <summary>
        /// Opens a new gamepad
        /// </summary>
        /// <param name="instance">The gamepad</param>
        public void OpenGamePad(int index)
        {
            if (index < 0 || index >= XInputGamePadCount)
                throw new IndexOutOfRangeException($"Invalid XInput device index {index}");
            if (devices[index] != null)
                throw new InvalidOperationException($"XInput device already opened {index}");

            var newGamepad = new GamePadXInput(controllers[index], controllerIds[index], index);
            newGamepad.OnDisconnect += (sender, args) =>
            {
                // Queue device for removal
                devicesToRemove.Add(index);
            };
            devices[index] = newGamepad;
            RegisterDevice(newGamepad);
        }

        public override void Dispose()
        {
            base.Dispose();

            // Dispose all the gamepads
            foreach (var gamePad in devices)
            {
                gamePad?.Dispose();
            }
        }
    }
}

#endif