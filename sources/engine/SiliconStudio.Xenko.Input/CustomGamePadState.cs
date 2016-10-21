// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Runtime.InteropServices;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    [DataFormat(DataFormatFlag.AbsoluteAxis)]
    public unsafe struct CustomGamePadStateRaw
    {
        private const DeviceObjectTypeFlags TypeRelativeAxisOpt =
            DeviceObjectTypeFlags.RelativeAxis | DeviceObjectTypeFlags.AbsoluteAxis | DeviceObjectTypeFlags.AnyInstance | DeviceObjectTypeFlags.Optional;

        private const DeviceObjectTypeFlags TypePovOpt = DeviceObjectTypeFlags.PointOfViewController | DeviceObjectTypeFlags.AnyInstance | DeviceObjectTypeFlags.Optional;
        private const DeviceObjectTypeFlags TypeButtonOpt = DeviceObjectTypeFlags.PushButton | DeviceObjectTypeFlags.ToggleButton | DeviceObjectTypeFlags.AnyInstance | DeviceObjectTypeFlags.Optional;

        public const int MaxSupportedButtons = 32;
        public const int MaxSupportedAxes = 32;
        public const int MaxSupportedPovControllers = 4;

        [DataObjectFormat(ArrayCount = MaxSupportedButtons, TypeFlags = TypeButtonOpt)] public fixed bool Buttons [32];
        [DataObjectFormat(ArrayCount = MaxSupportedAxes, TypeFlags = TypeRelativeAxisOpt)] public fixed int Axes [32];
        [DataObjectFormat(ArrayCount = MaxSupportedPovControllers, TypeFlags = TypePovOpt)] public fixed int PovControllers [4];
    }

    public class CustomGamePadState : IDeviceState<CustomGamePadStateRaw, JoystickUpdate>
    {
        public bool[] Buttons;
        public float[] Axes;
        public int[] PovControllers;

        public unsafe void MarshalFrom(ref CustomGamePadStateRaw value)
        {
            if (Axes.Length > CustomGamePadStateRaw.MaxSupportedAxes)
                throw new IndexOutOfRangeException("Axes are limited to 32 max");
            if (Buttons.Length > CustomGamePadStateRaw.MaxSupportedButtons)
                throw new IndexOutOfRangeException("Buttons are limited to 32 max");
            if (PovControllers.Length > CustomGamePadStateRaw.MaxSupportedPovControllers)
                throw new IndexOutOfRangeException("Point of view hats are limited to 32 max");
            fixed (int* axesPtr = value.Axes)
            {
                for (int i = 0; i < Axes.Length; i++)
                    Axes[i] = 2.0f*(axesPtr[i]/65535.0f - 0.5f);
            }
            fixed (bool* buttonsPtr = value.Buttons)
            {
                for (int i = 0; i < Buttons.Length; i++)
                    Buttons[i] = buttonsPtr[i];
            }
            fixed (int* hatsPtr = value.PovControllers)
            {
                for (int i = 0; i < PovControllers.Length; i++)
                    PovControllers[i] = hatsPtr[i];
            }
        }

        public void Update(JoystickUpdate update)
        {
        }
    }
}
#endif