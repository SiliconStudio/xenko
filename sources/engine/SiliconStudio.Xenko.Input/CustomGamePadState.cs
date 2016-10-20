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

        [DataObjectFormat(ArrayCount = 32, TypeFlags = TypeRelativeAxisOpt)]
        public fixed int Axes[32];
        [DataObjectFormat(ArrayCount = 32, TypeFlags = TypeButtonOpt)]
        public fixed bool Buttons[32];
        [DataObjectFormat(ArrayCount = 4, TypeFlags = TypePovOpt)]
        public fixed int Hats[4];
    }

    public class CustomGamePadState : IDeviceState<CustomGamePadStateRaw, JoystickUpdate>
    {
        public float[] Axes;
        public bool[] Buttons;
        public int[] Hats;

        public unsafe void MarshalFrom(ref CustomGamePadStateRaw value)
        {
            if(Axes.Length > 32)
                throw new IndexOutOfRangeException("Axes are limited to 32 max");
            if (Buttons.Length > 32)
                throw new IndexOutOfRangeException("Buttons are limited to 32 max");
            if (Hats.Length > 4)
                throw new IndexOutOfRangeException("Point of view hats are limited to 32 max");
            fixed (int* axesPtr = value.Axes)
            {
                for (int i = 0; i < Axes.Length; i++)
                    Axes[i] = 2.0f * (axesPtr[i] / 65535.0f - 0.5f);
            }
            fixed (bool* buttonsPtr = value.Buttons)
            {
                for (int i = 0; i < Buttons.Length; i++)
                    Buttons[i] = buttonsPtr[i];
            }
            fixed (int* hatsPtr = value.Hats)
            {
                for (int i = 0; i < Hats.Length; i++)
                    Hats[i] = hatsPtr[i];
            }
        }

        public void Update(JoystickUpdate update)
        {
        }
    }
}