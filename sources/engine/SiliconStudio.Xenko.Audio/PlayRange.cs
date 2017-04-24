// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Describes the range of audio samples to play, in time unit.
    /// </summary>
    public struct PlayRange
    {
        /// <summary>
        /// The Stating time.
        /// </summary>
        public TimeSpan Start;
        /// <summary>
        /// The Length of the audio extract to play.
        /// </summary>
        public TimeSpan Length;

        /// <summary>
        /// Creates a new PlayRange structure.
        /// </summary>
        /// <param name="start">The Stating time.</param>
        /// <param name="length">The Length of the audio extract to play.</param>
        public PlayRange(TimeSpan start, TimeSpan length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// The Ending time.
        /// </summary>
        public TimeSpan End
        {
            get { return Start + Length; }
            set { Length = value - Start; }
        }
    }
}
