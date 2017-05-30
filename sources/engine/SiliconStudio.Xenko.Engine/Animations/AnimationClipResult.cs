// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Updater;

namespace SiliconStudio.Xenko.Animations
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
        /// Stores all animation channel blittable struct at a given time.
        /// </summary>
        public byte[] Data = EmptyData;

        /// <summary>
        /// Stores all animation channel objects and non-blittable struct at a given time.
        /// </summary>
        public UpdateObjectData[] Objects;
    }
}
