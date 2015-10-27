// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// The different possible states of a gestures.
    /// </summary>
    public enum GestureState
    {
        /// <summary>
        /// A discrete gesture has occurred.
        /// </summary>
        Occurred,
        /// <summary>
        /// A continuous gesture has started.
        /// </summary>
        Began,
        /// <summary>
        /// A continuous gesture parameters changed.
        /// </summary>
        Changed,
        /// <summary>
        /// A continuous gesture has stopped.
        /// </summary>
        Ended,
    }
}