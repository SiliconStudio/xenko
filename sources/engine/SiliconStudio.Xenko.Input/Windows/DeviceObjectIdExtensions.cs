// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using SharpDX.DirectInput;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides easy operations on <see cref="DeviceObjectTypeFlags"/>
    /// </summary>
    internal static class DeviceObjectIdExtensions
    {
        public static bool HasFlags(this DeviceObjectId objectId, DeviceObjectTypeFlags flags)
        {
            return ((int)objectId.Flags & (int)flags) == (int)flags;
        }

        public static bool HasAnyFlag(this DeviceObjectId objectId, DeviceObjectTypeFlags flags)
        {
            return ((int)objectId.Flags & (int)flags) != 0;
        }
    }
}
#endif