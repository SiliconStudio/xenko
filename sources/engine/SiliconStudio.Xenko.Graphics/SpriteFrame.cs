// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A sprite frame represents a single frame a sprite animation.
    /// It contains the region of the texture representing the frame and the time it is supposed to be displayed.
    /// </summary>
    [DataContract]
    public class SpriteFrame
    {
        /// <summary>
        /// The rectangle specifying the region of the texture to use for that frame.
        /// </summary>
        public Rectangle TextureRegion;

        /// <summary>
        /// The bias to the frame center in pixels.
        /// </summary>
        public Vector2 CenterBias;

        /// <summary>
        /// Clone the current sprite frame instance.
        /// </summary>
        /// <returns>A new instance of the current sprite frame.</returns>
        public SpriteFrame Clone()
        {
            return (SpriteFrame)MemberwiseClone();
        }
    }
}
