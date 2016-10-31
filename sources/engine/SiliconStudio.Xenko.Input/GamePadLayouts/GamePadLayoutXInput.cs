// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System.Collections.Generic;
using SharpDX.XInput;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A Gamepad layout that matches an XBox360 controller, or any other controller that uses XInput
    /// </summary>
    public class GamePadLayoutXInput : GamePadLayout
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
            new GamePadAxisInfo { Name = "Left Trigger", IsBiDirectional = false },
            new GamePadAxisInfo { Name = "Right Trigger", IsBiDirectional = false },
        };

        public static readonly List<GamePadPovControllerInfo> PovControllers = new List<GamePadPovControllerInfo>
        {
            new GamePadPovControllerInfo { Name = "Pad" },
        };

        public override bool MatchDevice(IGamePadDevice device)
        {
            return device is GamePadXInput;
        }

        public override void GetState(IGamePadDevice device, ref GamePadState state)
        {
            var gamepad = ((GamePadXInput)device).state.Gamepad;
            
            // Buttons amp directly
            state.Buttons = (GamePadButton)gamepad.Buttons;

            // Apply deadzone reported by XInput controller
            float deadzoneL = Gamepad.LeftThumbDeadZone / 32768.0f;
            float deadzoneR = Gamepad.RightThumbDeadZone / 32768.0f;
            state.LeftThumb = new Vector2(GamePadUtils.ClampDeadZone(device.GetAxis(0), deadzoneL), GamePadUtils.ClampDeadZone(device.GetAxis(1), deadzoneL));
            state.RightThumb = new Vector2(GamePadUtils.ClampDeadZone(device.GetAxis(2), deadzoneR), GamePadUtils.ClampDeadZone(device.GetAxis(3), deadzoneR));

            state.LeftTrigger = device.GetAxis(4);
            state.RightTrigger = device.GetAxis(5);
        }
    }
}
#endif