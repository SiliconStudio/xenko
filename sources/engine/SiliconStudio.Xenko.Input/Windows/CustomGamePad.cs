// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    public class CustomGamePad : CustomDevice<CustomGamePadState, CustomGamePadStateRaw, JoystickUpdate>
    {
        public CustomGamePad(IntPtr nativePtr) : base(nativePtr)
        {
        }
        public CustomGamePad(DirectInput directInput, Guid deviceGuid) : base(directInput, deviceGuid)
        {
        }
    }
}
#endif