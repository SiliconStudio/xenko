// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    [Flags]
    public enum GamePadPadDirection
    {
        /// <summary>
        /// PadUp button.
        /// </summary>
        PadUp = 1 << 0,

        /// <summary>	
        /// PadRight button.
        /// </summary>	
        PadRight = 1 << 1,

        /// <summary>
        /// PadDown button.
        /// </summary>
        PadDown = 1 << 2,

        /// <summary>	
        /// PadLeft button.
        /// </summary>	
        PadLeft = 1 << 3,
    }

    public static class GamePadConversions
    {
        public static GamePadPadDirection PovControllerToPadDirection(float padValue)
        {
            GamePadPadDirection buttonState = 0;
            int dPadValue = (int)(padValue*8);
            switch (dPadValue)
            {
                case 0:
                    buttonState |= GamePadPadDirection.PadUp;
                    break;
                case 1:
                    buttonState |= GamePadPadDirection.PadUp;
                    buttonState |= GamePadPadDirection.PadRight;
                    break;
                case 2:
                    buttonState |= GamePadPadDirection.PadRight;
                    break;
                case 3:
                    buttonState |= GamePadPadDirection.PadRight;
                    buttonState |= GamePadPadDirection.PadDown;
                    break;
                case 4:
                    buttonState |= GamePadPadDirection.PadDown;
                    break;
                case 5:
                    buttonState |= GamePadPadDirection.PadDown;
                    buttonState |= GamePadPadDirection.PadLeft;
                    break;
                case 6:
                    buttonState |= GamePadPadDirection.PadLeft;
                    break;
                case 7:
                    buttonState |= GamePadPadDirection.PadLeft;
                    buttonState |= GamePadPadDirection.PadUp;
                    break;
            }
            return buttonState;
        }

        public static float PadDirectionToPovController(GamePadPadDirection padDirection)
        {
            int dpadValue = 0;
            for (int i = 0; i < 4; i++)
            {
                int maskBase = 1 << i;
                if (((int)padDirection & maskBase) != 0)
                {
                    dpadValue = i*2;

                    // The masks for the neighbouring buttons
                    int maskPos = 1 << ((i + 1)%4);
                    int maskNeg = 1 << (int)((uint)(i - 1)%4);

                    // Apply 2 button combinations by adjusting +1 or -1
                    if (((int)padDirection & maskPos) != 0)
                    {
                        dpadValue = (int)(((uint)dpadValue + 1)%8);
                    }
                    else if (((int)padDirection & maskNeg) != 0)
                    {
                        dpadValue = (int)(((uint)dpadValue - 1)%8);
                    }
                    break;
                }
            }

            return dpadValue/8.0f;
        }
    }
}