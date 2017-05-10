// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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