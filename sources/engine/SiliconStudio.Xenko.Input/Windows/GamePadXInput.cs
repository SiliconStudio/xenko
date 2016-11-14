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
        internal readonly Controller controller;
        internal State state;
        private readonly int index;
        
        public GamePadXInput(Controller controller, Guid id, int index)
        {
            this.controller = controller;
            this.index = index;
            Id = id;

            InitializeButtonStates();
            InitializeLayout();
        }
        
        public override string DeviceName => $"XInput Controller {index}";
        public override Guid Id { get; }

        public override IReadOnlyList<GamePadButtonInfo> ButtonInfos => GamePadLayoutXInput.Buttons;
        public override IReadOnlyList<GamePadAxisInfo> AxisInfos => GamePadLayoutXInput.Axes;
        public override IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos => GamePadLayoutXInput.PovControllers;

        public override void Update(List<InputEvent> inputEvents)
        {
            if (!Connected)
                return;

            if (controller.GetState(out state))
            {
                // Process DPad as point of view controller
                GamePadButton padDir = ConvertPadDirection(state.Gamepad.Buttons);
                float pov = GamePadUtils.ButtonToPovController(padDir);
                HandlePovController(0, pov, ((int)padDir & 0xF) != 0);

                // Start -> Right Shoulder button
                for (int i = 0; i < 6; i++)
                {
                    int mask = 1 << (i + 4);
                    bool buttonState = ((int)state.Gamepad.Buttons & mask) != 0;
                    HandleButton(i, buttonState);
                }

                // Face buttons
                for (int i = 0; i < 4; i++)
                {
                    int mask = 1 << (i + 12);
                    bool buttonState = ((int)state.Gamepad.Buttons & mask) != 0;
                    HandleButton(i+6, buttonState);
                }

                // Proces Axes
                HandleAxis(0, state.Gamepad.LeftThumbX/32768.0f);
                HandleAxis(1, state.Gamepad.LeftThumbY/32768.0f);

                HandleAxis(2, state.Gamepad.RightThumbX/32768.0f);
                HandleAxis(3, state.Gamepad.RightThumbY/32768.0f);

                HandleAxis(4, state.Gamepad.LeftTrigger/255.0f);
                HandleAxis(5, state.Gamepad.RightTrigger/255.0f);
            }

            if (!controller.IsConnected)
            {
                Dispose();
            }

            // Fire events
            base.Update(inputEvents);
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
    }
}
#endif