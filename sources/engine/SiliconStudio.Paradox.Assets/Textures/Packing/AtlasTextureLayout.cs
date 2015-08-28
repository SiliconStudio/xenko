using System.Collections.Generic;

namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// This specifies the full layout of an atlas texture.
    /// </summary>
    public class AtlasTextureLayout
    {
        /// <summary>
        /// Gets or sets a list of packed AtlasTextureElement
        /// </summary>
        public readonly List<AtlasTextureElement> Textures = new List<AtlasTextureElement>();

        /// <summary>
        /// Gets or sets Width of the texture atlas
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets Height of the texture atlas
        /// </summary>
        public int Height;
    }
}