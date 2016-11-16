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
    public class GamePadUWP : GameControllerDeviceBase
    {
        public override string DeviceName { get; }
        public override Guid Id { get; }
        public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }
        public override IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }
        public override IReadOnlyList<PovControllerInfo> PovControllerInfos { get; }
    }
}
#endif