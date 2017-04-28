// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    internal class DirectInputJoystick : CustomDevice<DirectInputState, RawJoystickState, JoystickUpdate>
    {
        public DirectInputJoystick(IntPtr nativePtr) : base(nativePtr)
        {
        }

        public DirectInputJoystick(DirectInput directInput, Guid deviceGuid) : base(directInput, deviceGuid)
        {
        }
    }
}
#endif