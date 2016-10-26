// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.XInput;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadXInput : GamePadDeviceBase, IGamePadVibration
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
            if (!Connected)
                return;

            if (controller.GetState(out state))
            {
                // Process DPad as point of view controller
                GamePadButton padDir = ConvertPadDirection(state.Gamepad.Buttons);
                float pov = GamePadUtils.ButtonToPovController(padDir);
                HandlePovController(0, pov, ((int)padDir & 0xF) != 0);

                // Process buttons
                for (int i = 0; i < ButtonInfos.Count; i++)
                {
                    int mask = 1 << (i + 4);
                    bool buttonState = ((int)state.Gamepad.Buttons & mask) != 0;
                    HandleButton(i, buttonState);
                }

                // Proces Axes
                HandleAxis(0, state.Gamepad.LeftThumbX/32768.0f);
                HandleAxis(1, state.Gamepad.LeftThumbY/32768.0f);

                HandleAxis(2, state.Gamepad.RightThumbX/32768.0f);
                HandleAxis(3, state.Gamepad.RightThumbY/32768.0f);

                HandleAxis(4, state.Gamepad.LeftTrigger/255.0f*2.0f - 1.0f);
                HandleAxis(5, state.Gamepad.RightTrigger/255.0f*2.0f - 1.0f);
            }

            if (!controller.IsConnected)
            {
                OnDisconnect?.Invoke(this, null);
                Dispose();
            }

            // Fire events
            base.Update();
        }

        public override bool GetGamePadState(ref GamePadState state)
        {
            state.Buttons = (GamePadButton)this.state.Gamepad.Buttons;
            state.LeftThumb = new Vector2(axisStates[0], axisStates[1]);
            state.RightThumb = new Vector2(axisStates[2], axisStates[3]);

            float deadzoneL = Gamepad.LeftThumbDeadZone/32768.0f;
            float deadzoneR = Gamepad.RightThumbDeadZone/32768.0f;
            state.LeftThumb = new Vector2(GamePadUtils.ClampDeadZone(GetAxis(0), deadzoneL), GamePadUtils.ClampDeadZone(GetAxis(1), deadzoneL));
            state.RightThumb = new Vector2(GamePadUtils.ClampDeadZone(GetAxis(2), deadzoneR), GamePadUtils.ClampDeadZone(GetAxis(3), deadzoneR));
            state.LeftTrigger = this.GetTrigger(4);
            state.RightTrigger = this.GetTrigger(5);

            // Direct mapping to GamePadState
            return true;
        }

        public void SetVibration(float leftMotor, float rightMotor)
        {
            if (!Connected)
                return;
            try
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
            catch (SharpDXException)
            {
                OnDisconnect?.Invoke(this, null);
                Dispose();
            }
        }

        public void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
                SetVibration((smallLeft + largeLeft)*0.5f, (smallRight + largeRight)*0.5f);
        }

        private GamePadButton ConvertPadDirection(GamepadButtonFlags buttons)
        {
            return (GamePadButton)buttons;
        }

        public class XBoxLayoutInfo
        {
            public static readonly List<GamePadButtonInfo> Buttons = new List<GamePadButtonInfo>
            {
                new GamePadButtonInfo { Name = "Start" },
                new GamePadButtonInfo { Name = "Back" },
                new GamePadButtonInfo { Name = "Left Thumb" },
                new GamePadButtonInfo { Name = "Right Thumb" },
                new GamePadButtonInfo { Name = "Left Shoulder" },
                new GamePadButtonInfo { Name = "Right Shoulder" },
                new GamePadButtonInfo { Name = "A" },
                new GamePadButtonInfo { Name = "B" },
                new GamePadButtonInfo { Name = "X" },
                new GamePadButtonInfo { Name = "Y" },
            };

            public static readonly List<GamePadAxisInfo> Axes = new List<GamePadAxisInfo>
            {
                new GamePadAxisInfo { Name = "Left Stick X" },
                new GamePadAxisInfo { Name = "Left Stick Y" },
                new GamePadAxisInfo { Name = "Right Stick X" },
                new GamePadAxisInfo { Name = "Right Stick Y" },
                new GamePadAxisInfo { Name = "Left Trigger" },
                new GamePadAxisInfo { Name = "Right Trigger" },
            };

            public static readonly List<GamePadPovControllerInfo> PovControllers = new List<GamePadPovControllerInfo>
            {
                new GamePadPovControllerInfo { Name = "Pad" },
            };
        }
    }
}
#endif