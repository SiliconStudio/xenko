// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace TopDownRPG.Core
{
    public static class InputManagerExtensions
    {
        public static bool IsGamePadButtonDown(this InputManager input, GamePadButton button, int index)
        {
            if (input.GamePadCount < index)
                return false;

            return (input.GetGamePad(index).State.Buttons & button) == button;
        }

        public static Vector2 GetLeftThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.LeftThumb : Vector2.Zero;
        }

        public static Vector2 GetRightThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.RightThumb : Vector2.Zero;
        }

        public static float GetLeftTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.LeftTrigger : 0.0f;
        }

        public static float GetRightTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.RightTrigger : 0.0f;
        }
    }
}
