// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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