// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;

namespace SiliconStudio.Xenko.Input
{
    public class KeyboardWinforms : KeyboardDeviceBase
    {
        public override string DeviceName => "Windows Keyboard";
        public override Guid Id => new Guid("027cf994-681f-4ed5-b38f-ce34fc295b8f");

        internal void HandleKeyDown(System.Windows.Forms.Keys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyDown(xenkoKey);
            }
        }
        internal void HandleKeyUp(System.Windows.Forms.Keys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyUp(xenkoKey);
            }
        }
    }
}
#endif