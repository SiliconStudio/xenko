using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Texture
{
    public class TexturePacker
    {
        private readonly MaxRectanglesBinPack maxRectPacker = new MaxRectanglesBinPack();
        private readonly List<TextureAtlas> textureAtlases = new List<TextureAtlas>();

        private Configuration packConfiguration;

        public void Initialize(Configuration configuration)
        {
            packConfiguration = configuration;

            maxRectPacker.Initialize(packConfiguration.MaxWidth, packConfiguration.MaxHeight, configuration.UseRotation);

            textureAtlases.Clear();
        }

        public void PackTextures(Dictionary<string, IntemediateTextureElement> textureElements)
        {
            // Create data for the packer
            var textureRegions = new List<RotatableRectangle>();

            foreach (var textureElementKey in textureElements.Keys)
            {
                var textureElement = textureElements[textureElementKey];

                textureRegions.Add(new RotatableRectangle
                {
                    Key = textureElementKey,
                    Value = new Rectangle(0, 0, textureElement.Texture.Width, textureElement.Texture.Height)
                });
            }

            // Pack
            maxRectPacker.Insert( textureRegions );

            if (textureRegions.Count > 0)
            {
                // todo:nut\ handle the case where the atlas could not fit all regions
            }
        }
    }

    public enum PivotType
    {
        Center,
        TopLeft,
    }

    public struct Configuration
    {
        public int BorderSize;

        public int ShapePaddingSize;

        public bool UseRotation;

        public bool UseMultipack;

        public PivotType PivotType;

        public int MaxWidth;

        public int MaxHeight;
    }

    public class IntemediateTextureElement
    {
        public string TextureName;

        public Texture2D Texture;

        public Rectangle Region;
    }

    public class TextureAtlas
    {
        public readonly List<IntemediateTextureElement> Textures = new List<IntemediateTextureElement>();

        public int Width;

        public int Height;

        public Configuration PackConfiguration;
    }


}
