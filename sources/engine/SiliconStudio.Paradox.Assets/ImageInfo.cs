using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Describes various information about an image.
    /// </summary>
    [DataContract("ImageInfo")]
    public class ImageInfo
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
        /// The name of the image instance.
        /// </userdoc>
        [DataMember(10)]
        public string Name;

        /// <summary>
        /// The rectangle specifying the region of the texture to use.
        /// </summary>
        /// <userdoc>
        /// The rectangle specifying the image region in the source file.
        /// </userdoc>
        [DataMember(20)]
        public Rectangle TextureRegion;

        /// <summary>
        /// Gets or sets the rotation to apply to the texture region when rendering the image
        /// </summary>
        /// <userdoc>
        /// The orientation of the image in the source file.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(ImageOrientation.AsIs)]
        public ImageOrientation Orientation { get; set; }
    }
}