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
        public static readonly List<GameControllerButtonInfo> Buttons = new List<GameControllerButtonInfo>
        {
            new GameControllerButtonInfo { Name = "Start" },
            new GameControllerButtonInfo { Name = "Back" },
            new GameControllerButtonInfo { Name = "Left Thumb" },
            new GameControllerButtonInfo { Name = "Right Thumb" },
            new GameControllerButtonInfo { Name = "Left Shoulder" },
            new GameControllerButtonInfo { Name = "Right Shoulder" },
            new GameControllerButtonInfo { Name = "A" },
            new GameControllerButtonInfo { Name = "B" },
            new GameControllerButtonInfo { Name = "X" },
            new GameControllerButtonInfo { Name = "Y" },
        };

        public static readonly List<GameControllerAxisInfo> Axes = new List<GameControllerAxisInfo>
        {
            new GameControllerAxisInfo { Name = "Left Stick X" },
            new GameControllerAxisInfo { Name = "Left Stick Y" },
            new GameControllerAxisInfo { Name = "Right Stick X" },
            new GameControllerAxisInfo { Name = "Right Stick Y" },
            new GameControllerAxisInfo { Name = "Left Trigger", IsBiDirectional = false },
            new GameControllerAxisInfo { Name = "Right Trigger", IsBiDirectional = false },
        };

        public static readonly List<GameControllerPovControllerInfo> PovControllers = new List<GameControllerPovControllerInfo>
        {
            new GameControllerPovControllerInfo { Name = "Pad" },
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

        public override bool MatchDevice(IGameControllerDevice device)
        {
            return device is GameControllerXInput;
        }
    }
}
#endif