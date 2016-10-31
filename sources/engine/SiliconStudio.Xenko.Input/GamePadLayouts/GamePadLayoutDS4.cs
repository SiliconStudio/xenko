// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

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

        public override bool MatchDevice(IGamePadDevice device)
        {
            var dinputDevice = device as GamePadDirectInput;
            if (dinputDevice != null)
            {
                return CompareProductId(dinputDevice.ProductId, commonProductId);
            }
            return false;
        }

        public override void GetState(IGamePadDevice device, ref GamePadState state)
        {
            // Provide default GamePadState mapping
            state.Buttons = 0;

            // Pov controller 0 as DPad
            state.Buttons |= device.GetDPad(0);

            // Map buttons using ds4ButtonMap
            foreach (var map in buttonMapping)
            {
                if (device.GetButton(map.Key))
                {
                    state.Buttons |= map.Value;
                }
            }

            // Convert axes while clamping deadzone
            state.LeftThumb = new Vector2(device.GetAxis(axisMapping[0]), -device.GetAxis(axisMapping[1]));
            state.RightThumb = new Vector2(device.GetAxis(axisMapping[2]), -device.GetAxis(axisMapping[3]));
            state.LeftTrigger = device.GetAxis(axisMapping[4]);
            state.RightTrigger = device.GetAxis(axisMapping[5]);
        }
    }
}

#endif