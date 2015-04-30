// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Paradox.Animations
{
    public class AnimationClipResult
    {
        private static readonly byte[] EmptyData = new byte[0];

        // Future use, when object will be supported.
        // private object[] objects;

        /// <summary>
        /// Total size of all structures to be stored in structures.
        /// </summary>
        public int DataSize;

        /// <summary>
        /// Gets or sets the animation channel descriptions.
        /// </summary>
        /// <value>
        /// The animation channel descriptions.
        /// </value>
        public List<AnimationBlender.Channel> Channels { get; set; }

        /// <summary>
        /// Stores all animation channel struct values at a given time.
        /// </summary>
        internal byte[] Data = EmptyData;
    }
}