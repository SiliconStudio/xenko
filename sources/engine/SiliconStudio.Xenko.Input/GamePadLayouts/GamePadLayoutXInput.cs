// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;
using SiliconStudio.Xenko.Native.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Layout for XInput devices so that they can be used by SDL or other systems that do not have the XInput API but do support joysticks in some other way
    /// </summary>
    public class GamePadLayoutXInput : GamePadLayout
    {
        private static readonly Guid commonProductId = new Guid("{706e6978-7475-0000-0000-000000000000}");

        public GamePadLayoutXInput()
        {
            AddButtonMapping(7, GamePadButton.Start);
            AddButtonMapping(6, GamePadButton.Back);
            AddButtonMapping(8, GamePadButton.LeftThumb);
            AddButtonMapping(9, GamePadButton.RightThumb);
            AddButtonMapping(4, GamePadButton.LeftShoulder);
            AddButtonMapping(5, GamePadButton.RightShoulder);
            AddButtonMapping(0, GamePadButton.A);
            AddButtonMapping(1, GamePadButton.B);
            AddButtonMapping(2, GamePadButton.X);
            AddButtonMapping(3, GamePadButton.Y);
            AddAxisMapping(0, GamePadAxis.LeftThumbX);
            AddAxisMapping(1, GamePadAxis.LeftThumbY, true);
            AddAxisMapping(3, GamePadAxis.RightThumbX);
            AddAxisMapping(4, GamePadAxis.RightThumbY, true);
            AddAxisMapping(2, GamePadAxis.LeftTrigger, remap: true);
            AddAxisMapping(5, GamePadAxis.RightTrigger, remap: true);
        }

        public override bool MatchDevice(IInputSource source, IGameControllerDevice device)
        {
            return CompareProductId(device.ProductId, commonProductId, 6);
        }
    }
}