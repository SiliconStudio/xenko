// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace VRSandbox.Core
{
    public static class InputManagerExtensions
    {
        public static bool IsGamePadButtonDown(this InputManager input, GamePadButton button, int index)
        {
            var gamepad = input.GetGamePad(index);
            if (gamepad == null)
                return false;

            return (gamepad.State.Buttons & button) == button;
        }
        
        public static Vector2 GetLeftThumb(this InputManager input, int index)
        {
            var gamepad = input.GetGamePad(index);
            if (gamepad == null)
                return Vector2.Zero;

            return gamepad.State.LeftThumb;
        }
        
        public static Vector2 GetRightThumb(this InputManager input, int index)
        {
            var gamepad = input.GetGamePad(index);
            if (gamepad == null)
                return Vector2.Zero;

            return gamepad.State.RightThumb;
        }
        
        public static float GetLeftTrigger(this InputManager input, int index)
        {
            var gamepad = input.GetGamePad(index);
            if (gamepad == null)
                return 0.0f;

            return gamepad.State.LeftTrigger;
        }
        
        public static float GetRightTrigger(this InputManager input, int index)
        {
            var gamepad = input.GetGamePad(index);
            if (gamepad == null)
                return 0.0f;

            return gamepad.State.RightTrigger;
        }
        
    }
}
