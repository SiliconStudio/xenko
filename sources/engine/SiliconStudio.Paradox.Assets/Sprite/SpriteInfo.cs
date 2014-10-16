// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// Describes a sprite asset.
    /// </summary>
    [DataContract("SpriteInfo")]
    public class SpriteInfo : ImageInfo
    {
        public SpriteInfo()
        {
            CenterFromMiddle = true;
        }

        /// <summary>
        /// The position of the center of the image in pixels.
        /// </summary>
        [DataMember(40)]
        public Vector2 Center;

        /// <summary>
        /// Gets or sets the value indicating position provided to <see cref="Center"/> is from the middle of the sprite region or from the left/top corner.
        /// </summary>
        [DataMember(50)]
        [DefaultValue(true)]
        public bool CenterFromMiddle { get; set; }
    }
}