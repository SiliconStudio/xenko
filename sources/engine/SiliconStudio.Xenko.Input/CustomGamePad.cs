// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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