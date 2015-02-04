using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A region of an image.
    /// </summary>
    [DataContract(Inherited = true)]
    public class ImageFragment
    {
        internal RectangleF RegionInternal;

        internal ImageFragment()
            : this(null)
        {
        }

        /// <summary>
        /// Creates an empty image fragment having the provided name.
        /// </summary>
        /// <param name="fragmentName">Name of the fragment</param>
        public ImageFragment(string fragmentName)
            :this(fragmentName, null, null)
        {
        }

        /// <summary>
        /// Creates a image fragment having the provided color/alpha textures and name.
        /// The region size is initialized with the whole size of the texture.
        /// </summary>
        /// <param name="fragmentName">Name of the fragment</param>
        /// <param name="color">The texture to use as color</param>
        /// <param name="alpha">the texture to use as alpha</param>
        public ImageFragment(string fragmentName, Texture color, Texture alpha)
        {
            Name = fragmentName;
            IsTransparent = true;

            Texture = color;
            TextureAlpha = alpha;

            var referenceTexture = color ?? alpha;
            if(referenceTexture != null)
                RegionInternal = new Rectangle(0, 0, referenceTexture.ViewWidth, referenceTexture.ViewHeight);
        }

        /// <summary>
        /// Gets or sets the name of the image fragment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The texture in which the image is contained
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// The texture in which the image alpha is contained
        /// </summary>
        public Texture TextureAlpha { get; set; }

        /// <summary>
        /// Gets a value indicating if the alpha component of the <see cref="ImageFragment"/> should be taken from the color of the <see cref="TextureAlpha"/> texture or not.
        /// </summary>
        public virtual bool SubstituteAlpha
        {
            get { return TextureAlpha != null; }
        }

        /// <summary>
        /// The rectangle specifying the region of the texture to use as fragment.
        /// </summary>
        public virtual RectangleF Region
        {
            get { return RegionInternal; }
            set { RegionInternal = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating if the fragment contains transparent regions.
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <summary>
        /// Gets or sets the rotation to apply to the texture region when rendering the <see cref="ImageFragment"/>
        /// </summary>
        public virtual ImageOrientation Orientation { get; set; }

        public override string ToString()
        {
            var textureName = Texture != null ? Texture.Name : "''";
            return Name + ", Texture: " + textureName + ", Region: " + Region;
        }
    }
}