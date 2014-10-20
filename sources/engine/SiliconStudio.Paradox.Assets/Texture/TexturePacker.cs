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

            textureAtlases.Clear();
        }

        public bool PackTextures(Dictionary<string, IntemediateTextureElement> textureElements)
        {
            // Create data for the packer
            var textureRegions = new List<RotatableRectangle>();

            foreach (var textureElementKey in textureElements.Keys)
            {
                var textureElement = textureElements[textureElementKey];
                textureRegions.Add(new RotatableRectangle(0, 0, textureElement.Texture.Width, textureElement.Texture.Height) { Key = textureElementKey });
            }

            do
            {
                // Reset packer state
                maxRectPacker.Initialize(packConfiguration.MaxWidth, packConfiguration.MaxHeight, packConfiguration.UseRotation);

                // Pack
                maxRectPacker.Insert(textureRegions);

                // Find true size from packed regions
                var trueSize = GetTrueSize(maxRectPacker.UsedRectangles);

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
            }
            while (packConfiguration.UseMultipack && textureRegions.Count > 0);

            return textureRegions.Count == 0;
        }

        private Size2 GetTrueSize(List<RotatableRectangle> usedRectangles)
        {
            // todo:nut\ Find the true size of this atlas
            return new Size2(packConfiguration.MaxWidth, packConfiguration.MaxHeight);
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
        public int BorderSize;

        public int ShapePaddingSize;

        public bool UseRotation;

        public bool UseMultipack;

        public PivotType PivotType;

        public SizeConstraints SizeContraint;

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
