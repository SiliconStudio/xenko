using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    public partial class TexturePacker
    {
        public List<TextureAtlas> TextureAtlases { get { return textureAtlases; } } 

        private readonly MaxRectanglesBinPack maxRectPacker = new MaxRectanglesBinPack();
        private readonly List<TextureAtlas> textureAtlases = new List<TextureAtlas>();

        private Config packConfig;

        public TexturePacker(Config config)
        {
            packConfig = config;

            textureAtlases.Clear();
        }

        public void ResetPacker(Config? config = null)
        {
            if(config != null) packConfig = (Config)config;

            textureAtlases.Clear();
        }

        /// <summary>
        /// Packs textureElement into textureAtlases.
        /// Note that, textureElements is modified when any texture element could be packed, it will be removed from the collection.
        /// </summary>
        /// <param name="textureElements">input texture elements</param>
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

        public enum SizeConstraints
        {
            Any,
            PowerOfTwo,
        }

        public struct Config
        {
            public bool HasBorder { get { return BorderSize > 0; } }

            public int BorderSize;

            public bool UseRotation;

            public bool UseMultipack;

            public SizeConstraints? SizeContraint;

            public TextureAddressMode? BorderAddressMode;

            public ImageFileType? OutputAtlasImageType;

            public Color? BorderColor;

            public int MaxWidth;

            public int MaxHeight;
        }

        public class IntermediateTexture
        {
            public Image Texture;

            public MaxRectanglesBinPack.RotatableRectangle Region;
        }

        public class TextureAtlas
        {
            public readonly List<IntermediateTexture> Textures = new List<IntermediateTexture>();

            public int Width;

            public int Height;

            public Config PackConfig;
        }
    }
}
