// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Internal interface used to assign gamepad index to a gamepad device
    /// </summary>
    internal interface IGamePadIndexAssignable : IGamePadDevice
    {
        new int Index { get; set; }
    }
}