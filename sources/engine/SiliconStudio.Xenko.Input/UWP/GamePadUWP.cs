// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_UWP
using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// UWP Gamepad
    /// </summary>
    public class GamePadUWP : GamePadDeviceBase
    {
        public override string DeviceName { get; }
        public override Guid Id { get; }
        public override IReadOnlyList<GamePadButtonInfo> ButtonInfos { get; }
        public override IReadOnlyList<GamePadAxisInfo> AxisInfos { get; }
        public override IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos { get; }
    }
}
#endif