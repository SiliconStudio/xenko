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

        public void Initialize(Configuration configuration)
        {
            packConfiguration = configuration;

            textureAtlases.Clear();
        }

        public bool PackTextures(Dictionary<string, IntemediateTextureElement> textureElements)
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
                maxRectPacker.Insert(textureRegions);

                // Find true size from packed regions
                var trueSize = GetTrueSize(maxRectPacker.UsedRectangles);

                // Alter the size of atlas so that it is a power of two
                if (packConfiguration.SizeContraint == SizeConstraints.PowerOfTwo)
                {
                    trueSize.Width = (int)Math.Pow(2, Math.Ceiling(Math.Log(trueSize.Width) / Math.Log(2)));
                    trueSize.Height = (int)Math.Pow(2, Math.Ceiling(Math.Log(trueSize.Height) / Math.Log(2)));

                    if (trueSize.Width > packConfiguration.MaxWidth || trueSize.Height > packConfiguration.MaxHeight)
                        return false;
                }

                // Insert the atlas to store packed regions
                var currentAtlas = new TextureAtlas
                {
                    PackConfiguration = packConfiguration,
                    Width = trueSize.Width,
                    Height = trueSize.Height,
                };

                // Insert all packed regions into Atlas
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
        private Size2 GetTrueSize(IEnumerable<RotatableRectangle> usedRectangles)
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
            // todo:nut\ make a pixel format a parameter so that a user could choose to create png or jpeg
            var atlasTexture = Texture2D.New(graphicsDevice, textureAtlas.Width, textureAtlas.Height, 1,
                PixelFormat.B8G8R8A8_UNorm, usage: GraphicsResourceUsage.Dynamic);

            var atlasTextureData = new ColorBGRA[textureAtlas.Width * textureAtlas.Height];

            // Fill in textureData from textureAtlas
            foreach (var intemediateTexture in textureAtlas.Textures)
            {
                var texture = intemediateTexture.Texture;

                var textureData = texture.GetData<ColorBGRA>();

                // If not use rotation, this copy use O(H). Otherwise, O(W*H) since the block of array is not contiguous when flipped
                for (var y = 0; y < texture.Height; ++y)
                {
                    if (!intemediateTexture.Region.IsRotated)
                    {
                        Array.Copy(textureData, y * texture.Width,
                            atlasTextureData, (intemediateTexture.Region.Value.Y + y) * textureAtlas.Width + intemediateTexture.Region.Value.X, texture.Width);

                        continue;
                    }

                    for (var x = 0; x < texture.Width; ++x)
                    {
                        var targetIndexX = intemediateTexture.Region.Value.X + (texture.Width - 1 - y);
                        var targetIndexY = intemediateTexture.Region.Value.Y + x;

                        atlasTextureData[targetIndexY * textureAtlas.Width + targetIndexX] = textureData[y * texture.Width + x];
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

        public Color BorderColor;

        public int MaxWidth;

        public int MaxHeight;
    }

    public class IntemediateTextureElement
    {
        public string TextureName;

        public Texture2D Texture;

        public RotatableRectangle Region;
    }

    public class TextureAtlas
    {
        public readonly List<IntemediateTextureElement> Textures = new List<IntemediateTextureElement>();

        public int Width;

        public int Height;

        public Configuration PackConfiguration;
    }


}
