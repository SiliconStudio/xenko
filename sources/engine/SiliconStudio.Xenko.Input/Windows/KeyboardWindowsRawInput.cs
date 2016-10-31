// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;

namespace SiliconStudio.Xenko.Input
{
    public class KeyboardWindowsRawInput : KeyboardDeviceBase
    {
        public override string DeviceName => "Windows Keyboard (Raw Input)";
        public override Guid Id => new Guid("d7437ff5-d14f-4491-9673-377b6d0e241c");
    }
}
#endif