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

        public GamePadLayoutXInput()
        {
            AddButtonMapping(0, GamePadButton.Start);
            AddButtonMapping(1, GamePadButton.Back);
            AddButtonMapping(2, GamePadButton.LeftThumb);
            AddButtonMapping(3, GamePadButton.RightThumb);
            AddButtonMapping(4, GamePadButton.LeftShoulder);
            AddButtonMapping(5, GamePadButton.RightShoulder);
            AddButtonMapping(6, GamePadButton.A);
            AddButtonMapping(7, GamePadButton.B);
            AddButtonMapping(8, GamePadButton.X);
            AddButtonMapping(9, GamePadButton.Y);
            AddAxisMapping(0, GamePadAxis.LeftThumbX);
            AddAxisMapping(1, GamePadAxis.LeftThumbY);
            AddAxisMapping(2, GamePadAxis.RightThumbX);
            AddAxisMapping(3, GamePadAxis.RightThumbY);
            AddAxisMapping(4, GamePadAxis.LeftTrigger);
            AddAxisMapping(5, GamePadAxis.RightTrigger);
        }

        public override bool MatchDevice(IGamePadDevice device)
        {
            return device is GamePadXInput;
        }
    }
}
#endif