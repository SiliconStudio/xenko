using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// TexturePacker class for packing several textures, using MaxRects <see cref="MaxRectanglesBinPack"/>, into one or more texture atlases
    /// </summary>
    public partial class TexturePacker
    {
        /// <summary>
        /// Gets or sets MaxRects heuristic algorithm to place rectangles
        /// </summary>
        public TexturePackingMethod Algorithm;

        /// <summary>
        /// Gets or sets the use of rotation for packing
        /// </summary>
        public bool UseRotation;

        /// <summary>
        /// Gets or sets the use of Multipack.
        /// If Multipack is enabled, a packer could create more than one texture atlases to fit all textures,
        /// whereas if Multipack is disabled, a packer always creates only one texture atlas which might not fit all textures.
        /// </summary>
        public bool UseMultipack;

        /// <summary>
        /// Gets or sets texture atlas size constraints: Any or Power of two
        /// - Any would create a texture exactly by a given size.
        /// - PowerOfTwo would create a texture where width and height are of power of two by ceiling a given size to the nearest power of two value.
        /// </summary>
        public AtlasSizeConstraints AtlasSizeContraint;

        /// <summary>
        /// Gets or sets MaxWidth for expected TextureAtlas
        /// </summary>
        public int MaxWidth;

        /// <summary>
        /// Gets or sets MaxHeight for expected TextureAtlas
        /// </summary>
        public int MaxHeight;

        /// <summary>
        /// Gets or sets border size
        /// </summary>
        public int BorderSize;

        /// <summary>
        /// Gets available Texture Atlases which contain a set of textures that are already packed
        /// </summary>
        public List<TextureAtlas> TextureAtlases { get { return textureAtlases; } }

        private readonly MaxRectanglesBinPack maxRectPacker = new MaxRectanglesBinPack();
        private readonly List<TextureAtlas> textureAtlases = new List<TextureAtlas>();

        /// <summary>
        /// Resets a packer states, and optional set a new Config
        /// </summary>
        public void ResetPacker()
        {
            textureAtlases.Clear();
        }

        public void ResetPacker(int maxWidth, int maxHeight, TexturePackingMethod algorithm = TexturePackingMethod.Best,
            bool useRotation = true, bool useMultipack = false, AtlasSizeConstraints atlasSizeConstraint = AtlasSizeConstraints.PowerOfTwo)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            Algorithm = algorithm;
            UseRotation = useRotation;
            UseMultipack = useMultipack;
            AtlasSizeContraint = atlasSizeConstraint;
        }

        /// <summary>
        /// Packs textureElement into textureAtlases
        /// </summary>
        /// <param name="textureElements"></param>
        /// <returns></returns>
        public bool PackTextures(Dictionary<string, IntermediateTexture> textureElements)
        {
            return TryPackTextures(textureElements, MaxWidth, MaxHeight);
        }

        private static List<Size2> CreateSubSizeArray(int maxWidth, int maxHeight, int startWidth, int startHeight)
        {
            var result = new List<Size2>();

            var currentWidth = (maxWidth > startWidth) ? startWidth : maxWidth;
            var currentHeight = (maxHeight > startHeight) ? startHeight : maxHeight;

            result.Add(new Size2(currentWidth, currentHeight));

            var selector = 0;

            while (currentWidth < maxWidth || currentHeight < maxHeight)
            {
                if (currentWidth < maxWidth && currentHeight < maxHeight)
                {
                    if (selector % 2 == 0)
                        currentWidth = 2 * currentWidth;
                    else
                        currentHeight = 2 * currentHeight;

                    ++selector;
                }
                else if (currentWidth < maxWidth)
                {
                    currentWidth = 2 * currentWidth;
                }
                else
                {
                    currentHeight = 2 * currentHeight;
                }

                result.Add(new Size2(currentWidth, currentHeight));
            }

            return result;
        }

        private bool TryPackTextures(Dictionary<string, IntermediateTexture> textureElements, int width, int height)
        {
            if (Algorithm == TexturePackingMethod.Best)
            {
                var results = new Dictionary<TexturePackingMethod, List<TextureAtlas>>();

                var bestAlgorithm = TexturePackingMethod.BestShortSideFit;
                var canPackAll = PackTextures(CloneIntermediateTextureDictionary(textureElements), bestAlgorithm, width, height);

                results[bestAlgorithm] = new List<TextureAtlas>(textureAtlases);

                foreach (var heuristicMethod in (TexturePackingMethod[])Enum.GetValues(typeof(TexturePackingMethod)))
                {
                    if (heuristicMethod == TexturePackingMethod.Best || heuristicMethod == TexturePackingMethod.BestShortSideFit)
                        continue;

                    ResetPacker();

                    // This algorithm can't pack all textures, so discard it 
                    if (!PackTextures(CloneIntermediateTextureDictionary(textureElements), heuristicMethod, width, height)) continue;

                    results[heuristicMethod] = new List<TextureAtlas>(textureAtlases);

                    if (CompareTextureAtlasLists(results[heuristicMethod], results[bestAlgorithm]) > 0 || !canPackAll)
                    {
                        canPackAll = true;
                        bestAlgorithm = heuristicMethod;
                    }
                }

                ResetPacker();

                if (canPackAll) textureAtlases.AddRange(results[bestAlgorithm]);

                return canPackAll;
            }

            ResetPacker();

            return PackTextures(textureElements, Algorithm, width, height);
        }

        /// <summary>
        /// Clones IntermediateTexture dictionary. It copies PackingRegion, but keep the same reference to the texture
        /// </summary>
        /// <param name="source">Prototype dictionary</param>
        /// <returns></returns>
        private static Dictionary<string, IntermediateTexture> CloneIntermediateTextureDictionary(Dictionary<string, IntermediateTexture> source)
        {
            return source.Keys.ToDictionary(key => key, key => new IntermediateTexture
            {
                PackingRegion = source[key].PackingRegion,
                Texture = source[key].Texture,
                Region = source[key].Region,
                AddressModeU = source[key].AddressModeU,
                AddressModeV = source[key].AddressModeV,
                BorderColor = source[key].BorderColor
            });
        }

        /// <summary>
        /// Compares two atlas List to check which list is more optimal in term of the number of atlas and areas
        /// </summary>
        /// <param name="atlasList1">Source 1</param>
        /// <param name="atlasList2">Source 2</param>
        /// <returns>Return -1 if atlasList1 is less optimal, 0 if the two list is the same level of optimal, 1 if atlasList1 is more optimal </returns>
        private int CompareTextureAtlasLists(List<TextureAtlas> atlasList1, List<TextureAtlas> atlasList2)
        {
            // Check the number of pages
            if (atlasList1.Count != atlasList2.Count)
                return (atlasList1.Count > atlasList2.Count) ? -1 : 1;

            // Check area
            var area1 = atlasList1.SelectMany(atlas => atlas.Textures).Sum(texture => texture.PackingRegion.Value.Width * texture.PackingRegion.Value.Height);
            var area2 = atlasList2.SelectMany(atlas => atlas.Textures).Sum(texture => texture.PackingRegion.Value.Width * texture.PackingRegion.Value.Height);

            if (area1 == area2)
                return 0;

            return (area1 > area2) ? -1 : 1;
        }

        /// <summary>
        /// Packs textureElement into textureAtlases, given heuristic algorithm
        /// </summary>
        /// <param name="textureElements">Input texture elements</param>
        /// <param name="algorithm">Packing algorithm</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>True indicates all textures could be packed; False otherwise</returns>
        public bool PackTextures(Dictionary<string, IntermediateTexture> textureElements, TexturePackingMethod algorithm, int width, int height)
        {
            var binWidth = (AtlasSizeContraint == AtlasSizeConstraints.PowerOfTwo) ? MathUtil.PreviousPowerOfTwo(width) : width;
            var binHeight = (AtlasSizeContraint == AtlasSizeConstraints.PowerOfTwo) ? MathUtil.PreviousPowerOfTwo(height) : height;

            // Create data for the packer
            var textureRegions = new List<RotatableRectangle>();

            foreach (var textureElementKey in textureElements.Keys)
            {
                var textureElement = textureElements[textureElementKey];

                textureRegions.Add(new RotatableRectangle(0, 0,
                    textureElement.Region.Width + 2 * BorderSize, textureElement.Region.Height + 2 * BorderSize) { Key = textureElementKey });
            }

            do
            {
                var sizeBeforePack = textureRegions.Count;

                var currentAtlas = TryCreateAtlas(textureElements, ref textureRegions, algorithm, binWidth, binHeight);

                if (textureRegions.Count == sizeBeforePack)
                    return false;

                if (textureRegions.Count > 0)
                {
                    foreach (var remainingTexture in textureRegions)
                    {
                        if (remainingTexture.Value.Width > width || remainingTexture.Value.Height > height)
                            return false;
                    }
                }

                textureAtlases.Add(currentAtlas);
            }
            while (UseMultipack && textureRegions.Count > 0);

            return textureRegions.Count == 0;
        }

        /// <summary>
        /// Try to pack as much as possible images in the given size.
        /// </summary>
        /// <returns>False if</returns>
        private TextureAtlas TryCreateAtlas(Dictionary<string, IntermediateTexture> textureElements, ref List<RotatableRectangle> textureRegions, TexturePackingMethod algorithm,
            int maxWidth, int maxHeight)
        {
            // Generate sub size array
            var subSizeArray = CreateSubSizeArray(maxWidth, maxHeight, 512, 512);

            var textureAtlas = new TextureAtlas { BorderSize = BorderSize };

            var bestPackNumber = int.MaxValue;

            var bestPackTextureRegions = textureRegions;

            foreach (var subArray in subSizeArray)
            {
                var currTextureRegions = new List<RotatableRectangle>(textureRegions);

                // Reset packer state
                maxRectPacker.Initialize(subArray.Width, subArray.Height, UseRotation);

                // Pack
                maxRectPacker.PackRectangles(currTextureRegions, algorithm);

                // Find true size from packed regions
                var packedSize = CalculatePackedRectanglesBound(maxRectPacker.PackedRectangles);

                // Alter the size of atlas so that it is a power of two
                if (AtlasSizeContraint == AtlasSizeConstraints.PowerOfTwo)
                {
                    packedSize.Width = MathUtil.NextPowerOfTwo(packedSize.Width);
                    packedSize.Height = MathUtil.NextPowerOfTwo(packedSize.Height);

                    if (packedSize.Width > subArray.Width || packedSize.Height > subArray.Height)
                        continue;
                }

                if (currTextureRegions.Count >= bestPackNumber)
                    continue;

                // Found new best pack, cache it
                bestPackNumber = currTextureRegions.Count;

                // Resize texture atlas
                textureAtlas.Width = packedSize.Width;
                textureAtlas.Height = packedSize.Height;

                textureAtlas.Textures.Clear();

                // Store all packed regions into Atlas
                foreach (var usedRectangle in maxRectPacker.PackedRectangles)
                {
                    textureElements[usedRectangle.Key].PackingRegion = usedRectangle;

                    textureAtlas.Textures.Add(textureElements[usedRectangle.Key]);
                }

                bestPackTextureRegions = currTextureRegions;
            }

            textureRegions = bestPackTextureRegions;
            return textureAtlas;
        }

        /// <summary>
        /// Calculates bound for the packed textures
        /// </summary>
        /// <param name="usedRectangles"></param>
        /// <returns></returns>
        private Size2 CalculatePackedRectanglesBound(IReadOnlyCollection<RotatableRectangle> usedRectangles)
        {
            if (usedRectangles.Count == 0) return Size2.Zero;

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

    /// <summary>
    /// Size constraints enums for TexturePacker where :
    /// - Any would create a texture exactly by a given size.
    /// - PowerOfTwo would create a texture where width and height are of power of two by ceiling a given size to the nearest power of two value.
    /// </summary>
    public enum AtlasSizeConstraints
    {
        PowerOfTwo,
        Any,
    }

    /// <summary>
    /// Intermediate texture element that is used during Packing and texture atlas creation processes.
    /// It represents a packed texture.
    /// </summary>
    public class IntermediateTexture
    {
        /// <summary>
        /// Gets or sets CPU-resource texture
        /// </summary>
        public Image Texture;

        /// <summary>
        /// Gets or sets region for packing
        /// </summary>
        public Rectangle Region;

        /// <summary>
        /// Gets or sets PackingRegion for the texture relative to a texture atlas that contains it
        /// </summary>
        public RotatableRectangle PackingRegion;

        /// <summary>
        /// Gets or sets border modes in X axis which applies specific TextureAddressMode in the border of each texture element in a given size of border
        /// </summary>
        public TextureAddressMode AddressModeU;

        /// <summary>
        /// Gets or sets border modes in Y axis which applies specific TextureAddressMode in the border of each texture element in a given size of border
        /// </summary>
        public TextureAddressMode AddressModeV;

        /// <summary>
        /// Gets or sets Border color when BorderModeU is set to Border mode
        /// </summary>
        public Color? BorderColor;
    }

    /// <summary>
    /// TextureAtlas contains packed intemediate textures, width and height of this atlas 
    /// </summary>
    public class TextureAtlas
    {
        /// <summary>
        /// Gets or sets a list of packed IntermediateTexture
        /// </summary>
        public readonly List<IntermediateTexture> Textures = new List<IntermediateTexture>();

        /// <summary>
        /// Gets or sets Width of the texture atlas
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets Height of the texture atlas
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets or sets Border size of an image
        /// </summary>
        public int BorderSize;
    }
}
