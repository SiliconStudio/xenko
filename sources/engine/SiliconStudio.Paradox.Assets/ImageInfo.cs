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
        public ImageInfo()
        {
            AddressModeU = TextureAddressMode.Border;
            AddressModeV = TextureAddressMode.Border;
        }

        /// <summary>
        /// Gets or sets the source file of this 
        /// </summary>
        /// <value>The source.</value>
        [DataMember(0)]
        [DefaultValue(null)]
        public UFile Source;

        /// <summary>
        /// Gets or sets the name of the sprite.
        /// </summary>
        [DataMember(10)]
        public string Name;

        /// <summary>
        /// The rectangle specifying the region of the texture to use.
        /// </summary>
        [DataMember(20)]
        public Rectangle TextureRegion;

        /// <summary>
        /// Gets or sets the rotation to apply to the texture region when rendering the image
        /// </summary>
        [DataMember(30)]
        [DefaultValue(ImageOrientation.AsIs)]
        public ImageOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or Sets atlas border mode in X axis for images inside atlas texture
        /// </summary>
        [DataMember(40)]
        public TextureAddressMode AddressModeU { get; set; }

        /// <summary>
        /// Gets or Sets atlas border mode in Y axis for images inside atlas texture
        /// </summary>
        [DataMember(50)]
        public TextureAddressMode AddressModeV { get; set; }

        /// <summary>
        /// Gets or Sets atlas border size for images inside atlas texture
        /// </summary>
        [DataMember(60)]
        [DefaultValue(0)]
        public int BorderSize { get; set; }

        /// <summary>
        /// Gets or Sets atlas border color for images inside atlas texture where Border mode is used in AddressModeU
        /// </summary>
        [DataMember(70)]
        public Color BorderColor { get; set; }
    }
}