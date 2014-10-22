using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    public class TexturePacker
    {
        public List<TextureAtlas> TextureAtlases { get { return textureAtlases; } } 

        private readonly MaxRectanglesBinPack maxRectPacker = new MaxRectanglesBinPack();
        private readonly List<TextureAtlas> textureAtlases = new List<TextureAtlas>();

        private Configuration packConfiguration;

        public TexturePacker(Configuration configuration)
        {
            packConfiguration = configuration;

            textureAtlases.Clear();
        }

        public void ResetPacker(Configuration? configuration = null)
        {
            if(configuration != null) packConfiguration = (Configuration)configuration;

            textureAtlases.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textureElements"></param>
        /// <returns></returns>
        public bool PackTextures(Dictionary<string, IntermediateTextureElement> textureElements)
        {
            // Create data for the packer
            var textureRegions = new List<RotatableRectangle>();

            foreach (var textureElementKey in textureElements.Keys)
            {
                var textureElement = textureElements[textureElementKey];
                textureRegions.Add(new RotatableRectangle(0, 0,
                    textureElement.Texture.Width + 2 * packConfiguration.BorderSize, textureElement.Texture.Height + 2 * packConfiguration.BorderSize) { Key = textureElementKey });
            }

            do
            {
                // Reset packer state
                maxRectPacker.Initialize(packConfiguration.MaxWidth, packConfiguration.MaxHeight, packConfiguration.UseRotation);

                // Pack
                maxRectPacker.PackRectangles(textureRegions);

                // Find true size from packed regions
                var packedSize = CalculatePackedRectanglesBound(maxRectPacker.UsedRectangles);

                // Alter the size of atlas so that it is a power of two
                if (packConfiguration.SizeContraint == SizeConstraints.PowerOfTwo)
                {
                    packedSize.Width = TextureCommandHelper.CeilingToNearestPowerOfTwo(packedSize.Width);
                    packedSize.Height = TextureCommandHelper.CeilingToNearestPowerOfTwo(packedSize.Height);

                    if (packedSize.Width > packConfiguration.MaxWidth || packedSize.Height > packConfiguration.MaxHeight)
                        return false;
                }

                // PackRectangles the atlas to store packed regions
                var currentAtlas = new TextureAtlas
                {
                    PackConfiguration = packConfiguration,
                    Width = packedSize.Width,
                    Height = packedSize.Height,
                };

                // PackRectangles all packed regions into Atlas
                foreach (var usedRectangle in maxRectPacker.UsedRectangles)
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
                        if(remainingTexture.Value.Width > packConfiguration.MaxWidth || remainingTexture.Value.Height > packConfiguration.MaxHeight)
                            return false;
                    }
                }
            }
            while (packConfiguration.UseMultipack && textureRegions.Count > 0);

            return textureRegions.Count == 0;
        }

        /// <summary>
        /// Calculates bound for the packed textures
        /// </summary>
        /// <param name="usedRectangles"></param>
        /// <returns></returns>
        private Size2 CalculatePackedRectanglesBound(IEnumerable<RotatableRectangle> usedRectangles)
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
    }

    public class TextureAtlasFactory
    {
        public static Texture2D CreateTextureAtlas(GraphicsDevice graphicsDevice, TextureAtlas textureAtlas)
        {
            var atlasTexture = Texture2D.New(graphicsDevice, textureAtlas.Width, textureAtlas.Height, 1,
                PixelFormat.B8G8R8A8_UNorm, usage: GraphicsResourceUsage.Dynamic);

            var atlasTextureData = new ColorBGRA[textureAtlas.Width * textureAtlas.Height];

            var borderSize = textureAtlas.PackConfiguration.BorderSize;

            // Fill in textureData from textureAtlas
            foreach (var intemediateTexture in textureAtlas.Textures)
            {
                var texture = intemediateTexture.Texture;

                var textureData = texture.GetData<ColorBGRA>();

                for (var y = -borderSize ; y < texture.Height + borderSize; ++y)
                {
                    for (var x = -borderSize; x < texture.Width + borderSize; ++x)
                    {
                        var targetIndexX = intemediateTexture.Region.Value.X + borderSize + ((intemediateTexture.Region.IsRotated) ? (texture.Width - 1 - y) : x);
                        var targetIndexY = intemediateTexture.Region.Value.Y + borderSize + ((intemediateTexture.Region.IsRotated) ? x : y);

                        var sourceIndexX = GetSourceTextureIndex(x, texture.Width, textureAtlas.PackConfiguration.BorderAddressMode);
                        var sourceIndexY = GetSourceTextureIndex(y, texture.Height, textureAtlas.PackConfiguration.BorderAddressMode);

                        atlasTextureData[targetIndexY * textureAtlas.Width + targetIndexX] = (sourceIndexX < 0 || sourceIndexY < 0)
                            ? (ColorBGRA)(textureAtlas.PackConfiguration.BorderColor ?? Color.Transparent) : textureData[sourceIndexY * texture.Width + sourceIndexX];
                    }
                }
            }

            // Update textureData to atlasTexture
            atlasTexture.SetData(atlasTextureData);

            return atlasTexture;
        }

        public static int GetSourceTextureIndex(int value, int maxValue, TextureAddressMode mode)
        {
            // Invariant condition
            if (0 <= value && value < maxValue) return value;

            switch (mode)
            {
                case TextureAddressMode.Wrap:
                    return (value >= 0) ? value % maxValue : (maxValue - ((-value) % maxValue)) % maxValue;
                case TextureAddressMode.Mirror:
                    return (value >= 0 ) ? (maxValue - 1) - (value % maxValue) : (-value) % maxValue;;
                case TextureAddressMode.Clamp:
                    return (value >= 0) ? maxValue - 1 : 0;
                case TextureAddressMode.MirrorOnce:
                    var absValue = Math.Abs(value);
                    if (0 <= absValue && absValue < maxValue) return absValue;
                    return (maxValue - 1) - (absValue % maxValue);
                case TextureAddressMode.Border:
                    return -1;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }

    public enum SizeConstraints
    {
        Any,
        PowerOfTwo,
    }

    public enum PivotType
    {
        Center,
        TopLeft,
    }

    public struct Configuration
    {
        public bool HasBorder{ get { return BorderSize > 0; } }

        public int BorderSize;

        public bool UseRotation;

        public bool UseMultipack;

        public PivotType PivotType;

        public SizeConstraints SizeContraint;

        public TextureAddressMode BorderAddressMode;

        public Color? BorderColor;

        public int MaxWidth;

        public int MaxHeight;
    }

    public class IntermediateTextureElement
    {
        public Texture2D Texture;

        public RotatableRectangle Region;
    }

    public class TextureAtlas
    {
        public readonly List<IntermediateTextureElement> Textures = new List<IntermediateTextureElement>();

        public int Width;

        public int Height;

        public Configuration PackConfiguration;
    }


}
