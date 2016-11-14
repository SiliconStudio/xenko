// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A gamepad layout for a DualShock4 controller
    /// </summary>
    public class GamePadLayoutDS4 : GamePadLayout
    {
        private static Guid commonProductId = new Guid(0x05c4054c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Dictionary<int, GamePadButton> buttonMapping = new Dictionary<int, GamePadButton>
        {
            { 9, GamePadButton.Start },
            { 8, GamePadButton.Back },
            { 10, GamePadButton.LeftThumb },
            { 11, GamePadButton.RightThumb },
            { 4, GamePadButton.LeftShoulder },
            { 5, GamePadButton.RightShoulder },
            { 1, GamePadButton.A },
            { 2, GamePadButton.B },
            { 0, GamePadButton.X },
            { 3, GamePadButton.Y },
        };

        private List<int> axisMapping = new List<int>
        {
            3,
            2, // L Stick
            1,
            0, // R Stick 
            5,
            4 // Triggers
        };

        private static bool CompareProductId(Guid a, Guid b)
        {
            byte[] aBytes = a.ToByteArray();
            byte[] bBytes = b.ToByteArray();
            for (int i = 0; i < 4; i++)
                if (aBytes[i] != bBytes[i]) return false;
            return true;
        }

        public GamePadLayoutDS4()
        {
            AddButtonMapping(9, GamePadButton.Start);
            AddButtonMapping(8, GamePadButton.Back);
            AddButtonMapping(10, GamePadButton.LeftThumb);
            AddButtonMapping(11, GamePadButton.RightThumb);
            AddButtonMapping(4, GamePadButton.LeftShoulder);
            AddButtonMapping(5, GamePadButton.RightShoulder);
            AddButtonMapping(1, GamePadButton.A);
            AddButtonMapping(2, GamePadButton.B);
            AddButtonMapping(0, GamePadButton.X);
            AddButtonMapping(3, GamePadButton.Y);
            AddAxisMapping(3, GamePadAxis.LeftThumbX);
            AddAxisMapping(2, GamePadAxis.LeftThumbY, true);
            AddAxisMapping(1, GamePadAxis.RightThumbX);
            AddAxisMapping(0, GamePadAxis.RightThumbY, true);
            AddAxisMapping(5, GamePadAxis.LeftTrigger);
            AddAxisMapping(4, GamePadAxis.RightTrigger);
        }

        public override bool MatchDevice(IGamePadDevice device)
        {
            var dinputDevice = device as GamePadDirectInput;
            if (dinputDevice != null)
            {
                return CompareProductId(dinputDevice.ProductId, commonProductId);
            }
            return false;
        }
    }
}

#endif