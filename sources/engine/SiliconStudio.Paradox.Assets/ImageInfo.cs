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
            AddressModeU = TextureAddressMode.Clamp;
            AddressModeV = TextureAddressMode.Clamp;
            BorderColor = Color.White;
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
        /// Gets or sets atlas border mode in X axis for images inside atlas texture
        /// </summary>
        [DataMember(40)]
        [DefaultValue(TextureAddressMode.Border)]
        public TextureAddressMode AddressModeU { get; set; }

        /// <summary>
        /// Gets or sets atlas border mode in Y axis for images inside atlas texture
        /// </summary>
        [DataMember(50)]
        [DefaultValue(TextureAddressMode.Border)]
        public TextureAddressMode AddressModeV { get; set; }

        /// <summary>
        /// Gets or sets atlas border color for images inside atlas texture where Border mode is used in AddressModeU
        /// </summary>
        [DataMember(60)]
        public Color BorderColor { get; set; }
    }
}