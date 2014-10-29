using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Texture;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal class ImageGroupCompiler<TGroupAsset, TImageInfo> : 
        AssetCompilerBase<TGroupAsset> 
        where TGroupAsset : ImageGroupAsset<TImageInfo> 
        where TImageInfo: ImageInfo
    {
        protected bool SeparateAlphaTexture;

        protected Dictionary<TImageInfo, int> SpriteToTextureIndex;

        private bool TextureFileIsValid(UFile file)
        {
            return file != null && File.Exists(file);
        }

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, TGroupAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep();
            
            // Evaluate if we need to use a separate the alpha texture
            SeparateAlphaTexture = context.Platform == PlatformType.Android && asset.Alpha != AlphaFormat.None && asset.Format == TextureFormat.Compressed;

            // create the registry containing the sprite assets texture index association
            SpriteToTextureIndex = new Dictionary<TImageInfo, int>();

            // create and add import texture commands
            if (asset.Images != null)
            {
                // return compilation error if one or more of the sprite does not have a valid texture
                var noSourceAsset = asset.Images.FirstOrDefault(x => !TextureFileIsValid(x.Source));
                if (noSourceAsset != null)
                {
                    result.Error("The texture of image '{0}' either does not exist or is invalid", noSourceAsset.Name);
                    return;
                }

                // sort sprites by referenced texture.
                var spriteByTextures = asset.Images.GroupBy(x => x.Source).ToArray();
                for (int i = 0; i < spriteByTextures.Length; i++)
                {
                    var spriteAssetArray = spriteByTextures[i].ToArray();
                    foreach (var spriteAsset in spriteAssetArray)
                        SpriteToTextureIndex[spriteAsset] = i;
                    
                    // texture asset does not need to be generated if using texture atlas
                    if(asset.GenerateTextureAtlas) continue;

                    // create an texture asset.
                    var textureAsset = new TextureAsset
                    {
                        Id = Guid.Empty, // CAUTION: It is important to use an empty GUID here, as we don't want the command to be rebuilt (by default, a new asset is creating a new guid)
                        Alpha = asset.Alpha,
                        Format = asset.Format,
                        GenerateMipmaps = asset.GenerateMipmaps,
                        PremultiplyAlpha = asset.PremultiplyAlpha,
                        ColorKeyColor = asset.ColorKeyColor,
                        ColorKeyEnabled = asset.ColorKeyEnabled,
                    };

                    // Get absolute path of asset source on disk
                    var assetDirectory = assetAbsolutePath.GetParent();
                    var assetSource = UPath.Combine(assetDirectory, spriteAssetArray[0].Source);

                    // add the texture build command.
                    result.BuildSteps.Add(
                        new TextureAssetCompiler.TextureConvertCommand(
                            ImageGroupAsset.BuildTextureUrl(urlInStorage, i),
                            new TextureConvertParameters(assetSource, textureAsset, context.Platform, context.GetGraphicsPlatform(), context.GetGraphicsProfile(), context.GetTextureQuality(), SeparateAlphaTexture)));
                }

                result.BuildSteps.Add(new WaitBuildStep()); // wait the textures to be imported
            }
        }
    }

    /// <summary>
    /// Command used to convert the texture in the storage
    /// </summary>
    public class ImageGroupCommand<TGroupAsset, TImageInfo, TImageGroupData, TImageData> : AssetCommand<ImageGroupParameters<TGroupAsset>>
        where TGroupAsset : ImageGroupAsset<TImageInfo>
        where TImageInfo : ImageInfo
        where TImageGroupData : ImageGroupData<TImageData>, new()
        where TImageData : ImageFragmentData, new()
    {
        protected readonly bool UseSeparateAlphaTexture;

        protected readonly Dictionary<TImageInfo, int> ImageToTextureIndex;

        protected ImageGroupCommand(string url, ImageGroupParameters<TGroupAsset> asset, Dictionary<TImageInfo, int> imageToTextureIndex, bool useSeparateAlphaTexture)
            : base(url, asset)
        {
            ImageToTextureIndex = imageToTextureIndex;
            UseSeparateAlphaTexture = useSeparateAlphaTexture;
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            for (int i = 0; i < ImageToTextureIndex.Values.Distinct().Count(); i++)
            {
                if (UseSeparateAlphaTexture)
                {
                    var textureUrl = ImageGroupAsset.BuildTextureUrl(Url, i);
                    yield return new ObjectUrl(UrlType.Internal, TextureAlphaComponentSplitter.GenerateColorTextureURL(textureUrl));
                    yield return new ObjectUrl(UrlType.Internal, TextureAlphaComponentSplitter.GenerateAlphaTextureURL(textureUrl));
                }
                else
                {
                    yield return new ObjectUrl(UrlType.Internal, ImageGroupAsset.BuildTextureUrl(Url, i));
                }
            }
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            var imageGroupData = new TImageGroupData { Images = new List<TImageData>() };

            // Create atlas texture
            Dictionary<string, Tuple<int, MaxRectanglesBinPack.RotatableRectangle>> regionDictionary = null;

            // Generate texture atlas
            if (asset.GroupAsset.GenerateTextureAtlas)
            {
                // Initialize packing configuration from GroupAsset
                var packConfiguration = new TexturePacker.Config
                {
                    Algorithm = asset.GroupAsset.AtlasPackingAlgorithm,
                    UseMultipack = asset.GroupAsset.UseMultipackAtlas,
                    UseRotation = asset.GroupAsset.UseRotationInAtlas,
                    MaxHeight = asset.GroupAsset.AtlasMaxHeight,
                    MaxWidth = asset.GroupAsset.AtlasMaxWidth,

                    // Enforce constraints
                    SizeContraint = TexturePacker.SizeConstraints.PowerOfTwo,
                };

                var resultStatus = CreateAndSaveTextureAtlasImage(ref packConfiguration, commandContext.Logger, out regionDictionary);

                if (resultStatus != ResultStatus.Successful) return Task.FromResult(resultStatus);
            }

            // Add the sprite data to the sprite list.
            foreach (var image in asset.GroupAsset.Images)
            {
                var newImage = new TImageData
                {
                    Name = image.Name,
                    IsTransparent = asset.GroupAsset.Alpha != AlphaFormat.None, // todo analyze texture region texture data to auto-determine alpha?
                };

                // Set region for each image
                if (asset.GroupAsset.GenerateTextureAtlas)
                {
                    var regionData = regionDictionary[ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image])];
                    var region = regionData.Item2;

                    newImage.Region = new Rectangle(image.BorderSize + region.Value.X, image.BorderSize + region.Value.Y, region.Value.Width - 2 * image.BorderSize, region.Value.Height - 2 * image.BorderSize);
                    newImage.Orientation = (region.IsRotated) ? ImageOrientation.Rotated90 : ImageOrientation.AsIs;
                }
                else
                {
                    newImage.Region = image.TextureRegion;
                    newImage.Orientation = image.Orientation;
                }

                if (UseSeparateAlphaTexture)
                {
                    var baseLocation = (asset.GroupAsset.GenerateTextureAtlas) 
                        ? ImageGroupAsset.BuildTextureAtlasUrl(Url, regionDictionary[ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image])].Item1) 
                        : ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image]);

                    newImage.Texture = new ContentReference<Texture2D> { Location = TextureAlphaComponentSplitter.GenerateColorTextureURL(baseLocation) };
                    newImage.TextureAlpha = new ContentReference<Texture2D> { Location = TextureAlphaComponentSplitter.GenerateAlphaTextureURL(baseLocation) };
                }
                else
                {
                    newImage.Texture = new ContentReference<Texture2D> { Location = (asset.GroupAsset.GenerateTextureAtlas) 
                        ? ImageGroupAsset.BuildTextureAtlasUrl(Url, regionDictionary[ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image])].Item1) 
                        : ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image]) };
                }

                SetImageSpecificFields(image, newImage);

                imageGroupData.Images.Add(newImage);
            }

            // save the imageData into the data base
            assetManager.Save(Url, imageGroupData);

            return Task.FromResult(ResultStatus.Successful);
        }

        /// <summary>
        /// Creates and Saves texture atlas image from images in GroupAsset
        /// </summary>
        /// <param name="packConfig">Packing configuration</param>
        /// <param name="logger">Status Logger</param>
        /// <param name="regionDictionary">Output that contains Key for each image and a tuple linking the image with a texture atlas index and its region</param>
        /// <returns>Status of building</returns>
        private ResultStatus CreateAndSaveTextureAtlasImage(ref TexturePacker.Config packConfig, Logger logger, 
            out Dictionary<string, Tuple<int, MaxRectanglesBinPack.RotatableRectangle>> regionDictionary)
        {
            regionDictionary = new Dictionary<string, Tuple<int, MaxRectanglesBinPack.RotatableRectangle>>();

            // Pack textures
            using (var texTool = new TextureTool())
            {
                var textureElements = new Dictionary<string, TexturePacker.IntermediateTexture>();

                foreach (var image in asset.GroupAsset.Images)
                    textureElements.Add(ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[image]), 
                        new TexturePacker.IntermediateTexture { Texture = LoadImage(texTool, new UFile(image.Source)), 
                            AddressModeU = image.AddressModeU, AddressModeV = image.AddressModeV, BorderSize = image.BorderSize, BorderColor = image.BorderColor});

                var texturePacker = new TexturePacker(packConfig);
                var canPackAllTextures = texturePacker.PackTextures(textureElements);

                if (!canPackAllTextures)
                {
                    logger.Error("Failed to pack all texture");
                    return ResultStatus.Failed;
                }

                // Create and save every generated texture atlas
                for (var textureAtlasIndex = 0 ; textureAtlasIndex < texturePacker.TextureAtlases.Count ; ++textureAtlasIndex)
                {
                    var textureAtlas = texturePacker.TextureAtlases[textureAtlasIndex];

                    var resultStatus = TexturePacker.Factory.CreateAndSaveTextureAtlasImage(textureAtlas, ImageGroupAsset.BuildTextureAtlasUrl(Url, textureAtlasIndex),
                        asset, UseSeparateAlphaTexture, CancellationToken, logger);

                    foreach (var texture in textureAtlas.Textures)
                    {
                        var textureKey = texture.Region.Key;

                        regionDictionary.Add(textureKey, new Tuple<int, MaxRectanglesBinPack.RotatableRectangle>(textureAtlasIndex, texture.Region));

                        texture.Texture.Dispose();
                    }

                    if (resultStatus != ResultStatus.Successful) return resultStatus;
                }
            }

            return ResultStatus.Successful;
        }

        protected virtual void SetImageSpecificFields(TImageInfo imageInfo, TImageData newImage)
        {
        }

        /// <summary>
        /// Loads image from a path with texTool
        /// </summary>
        /// <param name="texTool">A tool for loading an image</param>
        /// <param name="sourcePath">Source path of an image</param>
        /// <returns></returns>
        private static Image LoadImage(TextureTool texTool, UFile sourcePath)
        {
            using (var texImage = texTool.Load(sourcePath))
            {
                if (texImage.Format == PixelFormat.B8G8R8A8_UNorm)
                    texTool.SwitchChannel(texImage);

                return texTool.ConvertToParadoxImage(texImage);
            }
        }
    }
    
    /// <summary>
    /// Parameters used for converting/processing the texture in the storage.
    /// </summary>
    [DataContract]
    public class ImageGroupParameters<T>
    {
        public ImageGroupParameters()
        {
        }

        public ImageGroupParameters(T groupAsset, PlatformType platform, GraphicsPlatform graphicsPlatform, GraphicsProfile graphicsProfile, TextureQuality textureQuality)
        {
            GroupAsset = groupAsset;
            Platform = platform;
            GraphicsPlatform = graphicsPlatform;
            GraphicsProfile = graphicsProfile;
            TextureQuality = textureQuality;
        }

        public T GroupAsset;
    
        public PlatformType Platform;

        public GraphicsPlatform GraphicsPlatform;

        public GraphicsProfile GraphicsProfile;

        public TextureQuality TextureQuality;
    }
}