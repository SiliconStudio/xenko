// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An event that was generated from an <see cref="IInputDevice"/>
    /// </summary>
    public abstract class InputEvent
    {
        /// <summary>
        /// The device that sent this event
        /// </summary>
        public IInputDevice Device { get; protected internal set; }
    }
}