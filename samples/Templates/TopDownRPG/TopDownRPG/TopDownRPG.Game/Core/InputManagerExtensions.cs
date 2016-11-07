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

            return (input.GetGamePadState(index).Buttons & button) == button;
        }

        public static Vector2 GetLeftThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePadState(index).LeftThumb : Vector2.Zero;
        }

        public static Vector2 GetRightThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePadState(index).RightThumb : Vector2.Zero;
        }

        public static float GetLeftTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePadState(index).LeftTrigger : 0.0f;
        }

        public static float GetRightTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePadState(index).RightTrigger : 0.0f;
        }
    }
}
