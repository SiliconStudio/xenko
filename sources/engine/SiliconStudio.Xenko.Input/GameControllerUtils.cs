// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides some useful functions relating to game controllers
    /// </summary>
    public static class GameControllerUtils
    {
        /// <summary>
        /// Applies a deadzone to an axis input value
        /// </summary>
        /// <param name="value">The axis input value</param>
        /// <param name="deadZone">The deadzone treshold</param>
        /// <returns>The axis value with the applied deadzone</returns>
        public static float ClampDeadZone(float value, float deadZone)
        {
            if (value > 0.0f)
            {
                value -= deadZone;
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
            }
            else
            {
                value += deadZone;
                if (value > 0.0f)
                {
                    value = 0.0f;
                }
            }

            // Renormalize the value according to the dead zone
            value = value/(1.0f - deadZone);
            return value < -1.0f ? -1.0f : value > 1.0f ? 1.0f : value;
        }
        
        /// <summary>
        /// Returns the button next to this DPad button, going clockwise
        /// </summary>
        /// <param name="start">The starting button</param>
        /// <returns>The button that lies next to this button</returns>
        public static GamePadButton NextGamePadButtonCW(GamePadButton start)
        {
            switch (start)
            {
                default:
                case GamePadButton.PadUp:
                    return GamePadButton.PadRight;
                case GamePadButton.PadDown:
                    return GamePadButton.PadLeft;
                case GamePadButton.PadLeft:
                    return GamePadButton.PadUp;
                case GamePadButton.PadRight:
                    return GamePadButton.PadDown;
            }
        }

        /// <summary>
        /// Returns the button next to this DPad button, going counter-clockwise
        /// </summary>
        /// <param name="start">The starting button</param>
        /// <returns>The button that lies next to this button</returns>
        public static GamePadButton NextGamePadButtonCCW(GamePadButton start)
        {
            switch (start)
            {
                default:
                case GamePadButton.PadUp:
                    return GamePadButton.PadLeft;
                case GamePadButton.PadDown:
                    return GamePadButton.PadRight;
                case GamePadButton.PadLeft:
                    return GamePadButton.PadDown;
                case GamePadButton.PadRight:
                    return GamePadButton.PadUp;
            }
        }

        /// <summary>
        /// Converts a point of view controller's value to a combination of <see cref="GamePadButton"/>'s
        /// </summary>
        /// <param name="padValue">The pov controller's direction from 0 to 1 where 0 is up, rotating clockwise</param>
        /// <returns>A <see cref="GamePadButton"/> that has the values set to matching the input direction</returns>
        public static GamePadButton PovControllerToButton(float padValue)
        {
            GamePadButton buttonState = 0;
            int dPadValue = (int)(padValue*8);
            switch (dPadValue)
            {
                case 0:
                    buttonState |= GamePadButton.PadUp;
                    break;
                case 1:
                    buttonState |= GamePadButton.PadUp;
                    buttonState |= GamePadButton.PadRight;
                    break;
                case 2:
                    buttonState |= GamePadButton.PadRight;
                    break;
                case 3:
                    buttonState |= GamePadButton.PadRight;
                    buttonState |= GamePadButton.PadDown;
                    break;
                case 4:
                    buttonState |= GamePadButton.PadDown;
                    break;
                case 5:
                    buttonState |= GamePadButton.PadDown;
                    buttonState |= GamePadButton.PadLeft;
                    break;
                case 6:
                    buttonState |= GamePadButton.PadLeft;
                    break;
                case 7:
                    buttonState |= GamePadButton.PadLeft;
                    buttonState |= GamePadButton.PadUp;
                    break;
            }
            return buttonState;
        }

        /// <summary>
        /// Converts the pad buttons of a <see cref="GamePadButton"/> to a direction from 0 to 1
        /// </summary>
        /// <param name="padDirection">The pad buttons that need to be converted</param>
        /// <returns>A value from 0 to 1 indicating a direction where 0 is up, rotating clockwise</returns>
        public static float ButtonToPovController(GamePadButton padDirection)
        {
            int dpadValue = 0;
            for (int i = 0; i < 4; i++)
            {
                int mask = 1 << i;
                if (((int)padDirection & mask) != 0)
                {
                    switch ((GamePadButton)mask)
                    {
                        case GamePadButton.PadUp:
                            dpadValue = 0;
                            break;
                        case GamePadButton.PadRight:
                            dpadValue = 2;
                            break;
                        case GamePadButton.PadDown:
                            dpadValue = 4;
                            break;
                        case GamePadButton.PadLeft:
                            dpadValue = 6;
                            break;
                    }

                    // The masks for the neighbouring buttons
                    int maskPos = (int)NextGamePadButtonCW((GamePadButton)mask);
                    int maskNeg = (int)NextGamePadButtonCCW((GamePadButton)mask);

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