using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    /// <summary>
    /// TexturePacker class for packing several textures, using MaxRects (MaxRectanglesBinPack), into one or more texture atlases
    /// </summary>
    public partial class TexturePacker
    {
        /// <summary>
        /// Gets available Texture Atlases which contain a set of textures that are already packed
        /// </summary>
        public List<TextureAtlas> TextureAtlases { get { return textureAtlases; } }

        private Config packConfig;
        private readonly MaxRectanglesBinPack maxRectPacker = new MaxRectanglesBinPack();
        private readonly List<TextureAtlas> textureAtlases = new List<TextureAtlas>();

        /// <summary>
        /// Initializes a new instance of TexturePacker by a given pack Config
        /// </summary>
        /// <param name="config">Pack configuration</param>
        public TexturePacker(Config config)
        {
            packConfig = config;

            textureAtlases.Clear();
        }

        /// <summary>
        /// Resets a packer states, and optional set a new Config
        /// </summary>
        /// <param name="config">Pack configuration</param>
        public void ResetPacker(Config? config = null)
        {
            if(config != null) packConfig = (Config)config;

            textureAtlases.Clear();
        }

        /// <summary>
        /// Packs textureElement into textureAtlases.
        /// Note that, textureElements is modified when any texture element could be packed, it will be removed from the collection.
        /// </summary>
        /// <param name="textureElements">Input texture elements</param>
        /// <returns>True indicates all textures could be packed; False otherwise</returns>
        public bool PackTextures(Dictionary<string, IntermediateTexture> textureElements)
        {
            // Create data for the packer
            var textureRegions = new List<MaxRectanglesBinPack.RotatableRectangle>();

            foreach (var textureElementKey in textureElements.Keys)
            {
                var textureElement = textureElements[textureElementKey];

                textureRegions.Add(new MaxRectanglesBinPack.RotatableRectangle(0, 0,
                    textureElement.Texture.Description.Width + 2 * packConfig.BorderSize, textureElement.Texture.Description.Height + 2 * packConfig.BorderSize) { Key = textureElementKey });
            }

            do
            {
                // Reset packer state
                maxRectPacker.Initialize(packConfig.MaxWidth, packConfig.MaxHeight, packConfig.UseRotation);

                // Pack
                maxRectPacker.PackRectangles(textureRegions);

                // Find true size from packed regions
                var packedSize = CalculatePackedRectanglesBound(maxRectPacker.PackedRectangles);

                // Alter the size of atlas so that it is a power of two
                if (packConfig.SizeContraint == SizeConstraints.PowerOfTwo || packConfig.SizeContraint == null)
                {
                    packedSize.Width = TextureCommandHelper.CeilingToNearestPowerOfTwo(packedSize.Width);
                    packedSize.Height = TextureCommandHelper.CeilingToNearestPowerOfTwo(packedSize.Height);

                    if (packedSize.Width > packConfig.MaxWidth || packedSize.Height > packConfig.MaxHeight)
                        return false;
                }

                // PackRectangles the atlas to store packed regions
                var currentAtlas = new TextureAtlas
                {
                    PackConfig = packConfig,
                    Width = packedSize.Width,
                    Height = packedSize.Height,
                };

                // PackRectangles all packed regions into Atlas
                foreach (var usedRectangle in maxRectPacker.PackedRectangles)
                {
                    textureElements[usedRectangle.Key].Region = usedRectangle;

                    currentAtlas.Textures.Add(textureElements[usedRectangle.Key]);

                    textureElements.Remove(usedRectangle.Key);
                }

                textureAtlases.Add( currentAtlas );

                if (textureRegions.Count > 0)
                {
                    foreach (var remainingTexture in textureRegions)
                    {
                        if(remainingTexture.Value.Width > packConfig.MaxWidth || remainingTexture.Value.Height > packConfig.MaxHeight)
                            return false;
                    }
                }
            }
            while (packConfig.UseMultipack && textureRegions.Count > 0);

            return textureRegions.Count == 0;
        }

        /// <summary>
        /// Calculates bound for the packed textures
        /// </summary>
        /// <param name="usedRectangles"></param>
        /// <returns></returns>
        private Size2 CalculatePackedRectanglesBound(IEnumerable<MaxRectanglesBinPack.RotatableRectangle> usedRectangles)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;

            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach (var useRectangle in usedRectangles)
            {
                if (minX > useRectangle.Value.X) minX = useRectangle.Value.X;
                if (minY > useRectangle.Value.Y) minY = useRectangle.Value.Y;

                if (maxX < useRectangle.Value.X + useRectangle.Value.Width) maxX = useRectangle.Value.X + useRectangle.Value.Width;
                if (maxY < useRectangle.Value.Y + useRectangle.Value.Height) maxY = useRectangle.Value.Y + useRectangle.Value.Height;
            }

            return new Size2(maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Size constraints enums for TexturePacker where :
        /// - Any would create a texture exactly by a given size.
        /// - PowerOfTwo would create a texture where width and height are of power of two by ceiling a given size to the nearest power of two value.
        /// </summary>
        public enum SizeConstraints
        {
            Any,
            PowerOfTwo,
        }

        /// <summary>
        /// Packing Configuration defines constraints for TexturePacker
        /// </summary>
        public struct Config
        {
            /// <summary>
            /// Gets a boolean indicating if border is enabled
            /// </summary>
            public bool HasBorder { get { return BorderSize > 0; } }

            /// <summary>
            /// Gets or Sets border size
            /// </summary>
            public int BorderSize;

            /// <summary>
            /// Gets or Sets the use of rotation for packing
            /// </summary>
            public bool UseRotation;

            /// <summary>
            /// Gets or Sets the use of Multipack.
            /// If Multipack is enabled, a packer could create more than one texture atlases to fit all textures,
            /// whereas if Multipack is disabled, a packer always creates only one texture atlas which might not fit all textures.
            /// </summary>
            public bool UseMultipack;

            /// <summary>
            /// Gets or Sets texture atlas size constraints: Any or Power of two
            /// - Any would create a texture exactly by a given size.
            /// - PowerOfTwo would create a texture where width and height are of power of two by ceiling a given size to the nearest power of two value.
            /// </summary>
            public SizeConstraints? SizeContraint;

            /// <summary>
            /// Gets or Sets border modes which applies specific TextureAddressMode in the border of each texture element in a given size of border
            /// </summary>
            public TextureAddressMode? BorderAddressMode;

            /// <summary>
            /// Gets or Sets output image type of texture atlas
            /// </summary>
            public ImageFileType? OutputAtlasImageType;

            /// <summary>
            /// Gets or Sets Border color when BorderAddressMode is set to Border mode
            /// </summary>
            public Color? BorderColor;

            /// <summary>
            /// Gets or Sets MaxWidth for expected TextureAtlas
            /// </summary>
            public int MaxWidth;

            /// <summary>
            /// Gets or Sets MaxHeight for expected TextureAtlas
            /// </summary>
            public int MaxHeight;
        }

        /// <summary>
        /// Intermediate texture element that is used during Packing and texture atlas creation processes.
        /// It represents a packed texture.
        /// </summary>
        public class IntermediateTexture
        {
            /// <summary>
            /// Gets or Sets CPU-resource texture
            /// </summary>
            public Image Texture;

            /// <summary>
            /// Gets or Sets Region for the texture relative to a texture atlas that contains it
            /// </summary>
            public MaxRectanglesBinPack.RotatableRectangle Region;
        }

        /// <summary>
        /// TextureAtlas contains packed intemediate textures, width and height of this atlas 
        /// </summary>
        public class TextureAtlas
        {
            /// <summary>
            /// Gets or Sets a list of packed IntermediateTexture
            /// </summary>
            public readonly List<IntermediateTexture> Textures = new List<IntermediateTexture>();

            /// <summary>
            /// Gets or Sets Width of the texture atlas
            /// </summary>
            public int Width;

            /// <summary>
            /// Gets or Sets Height of the texture atlas
            /// </summary>
            public int Height;

            /// <summary>
            /// Gets or Sets Packing configuration
            /// </summary>
            public Config PackConfig;
        }
    }
}
