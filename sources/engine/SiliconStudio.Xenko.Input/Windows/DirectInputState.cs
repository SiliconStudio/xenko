// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    internal class DirectInputState : IDeviceState<RawJoystickState, JoystickUpdate>
    {
        public bool[] Buttons = new bool[128];
        public float[] Axes = new float[8];
        public int[] PovControllers = new int[4];

        public unsafe void MarshalFrom(ref RawJoystickState value)
        {
            fixed (int* axesPtr = &value.X)
            {
                for (int i = 0; i < 8; i++)
                    Axes[i] = axesPtr[i]/65535.0f;
            }
            fixed (byte* buttonsPtr = value.Buttons)
            {
                for (int i = 0; i < Buttons.Length; i++)
                    Buttons[i] = buttonsPtr[i] != 0;
            }
            fixed (int* povPtr = value.PointOfViewControllers)
            {
                for (int i = 0; i < PovControllers.Length; i++)
                    PovControllers[i] = povPtr[i];
            }
        }

        public void Update(JoystickUpdate update)
        {
        }
    }
}
#endif