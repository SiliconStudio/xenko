// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// This class contains all the information to describe one sprite.
    /// </summary>
    [DataContract("SpriteInfo")]
    public class SpriteInfo
    {
        /// <summary>
        /// Gets or sets the source file of this 
        /// </summary>
        /// <value>The source.</value>
        /// <userdoc>
        /// The path to the file containing the image data.
        /// </userdoc>
        [DataMember(0)]
        [DefaultValue(null)]
        public UFile Source;

        /// <summary>
        /// Gets or sets the name of the sprite.
        /// </summary>
        /// <userdoc>
        /// The name of the sprite instance.
        /// </userdoc>
        [DataMember(10)]
        public string Name;

        /// <summary>
        /// The rectangle specifying the region of the texture to use.
        /// </summary>
        /// <userdoc>
        /// The rectangle specifying the sprite region in the source file.
        /// </userdoc>
        [DataMember(20)]
        public Rectangle TextureRegion;

        /// <summary>
        /// The number of pixels representing an unit of 1 in the scene.
        /// </summary>
        /// <userdoc>
        /// The number of pixels representing an unit of 1 in the scene.
        /// </userdoc>
        [DataMember(25)]
        [DefaultValue(100)]
        public float PixelsPerUnit;

        /// <summary>
        /// Gets or sets the rotation to apply to the texture region when rendering the sprite
        /// </summary>
        /// <userdoc>
        /// The orientation of the sprite in the source file.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(ImageOrientation.AsIs)]
        public ImageOrientation Orientation { get; set; }

        /// <summary>
        /// The position of the center of the sprite in pixels.
        /// </summary>
        /// <userdoc>
        /// The position of the center of the sprite in pixels. 
        /// Depending on the value of 'CenterFromMiddle', it is the offset from the top/left corner or the middle of the sprite.
        /// </userdoc>
        [DataMember(40)]
        public Vector2 Center;

        /// <summary>
        /// Gets or sets the value indicating position provided to <see cref="Center"/> is from the middle of the sprite region or from the left/top corner.
        /// </summary>
        /// <userdoc>
        /// If checked, the value in 'Center' represents the offset of the sprite center from the middle of the sprite.
        /// </userdoc>
        [DataMember(50)]
        [DefaultValue(true)]
        public bool CenterFromMiddle { get; set; }

        /// <summary>
        /// Gets or sets the size of the non-stretchable borders of the sprite.
        /// </summary>
        /// <userdoc>
        /// The size in pixels of the non-stretchable parts of the sprite.
        /// The part sizes are organized as follow: X->Left, Y->Right, Z->Top, W->Bottom.
        /// </userdoc>
        [DataMember(60)]
        public Vector4 Borders { get; set; }

        /// <summary>
        /// Creates an empty instance of SpriteInfo
        /// </summary>
        public SpriteInfo()
        {
            PixelsPerUnit = 100;
            CenterFromMiddle = true;
        }
    }
}