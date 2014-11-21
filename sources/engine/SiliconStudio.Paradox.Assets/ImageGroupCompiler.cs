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

        protected Dictionary<TImageInfo, string> SpriteToTextureKey;

        private bool TextureFileIsValid(UFile file)
        {
            return file != null && File.Exists(file);
        }

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, TGroupAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new ListBuildStep();

            // TODO: temporary disable compress mode for the texture in Android; consequently there's no separate alpha. Since, SpriteBatch does not support currently.
            // Evaluate if we need to use a separate the alpha texture
            // SeparateAlphaTexture = context.Platform == PlatformType.Android && asset.Alpha != AlphaFormat.None && asset.Format == TextureFormat.Compressed;

            SeparateAlphaTexture = false;

            if (context.Platform == PlatformType.Android && asset.Format == TextureFormat.Compressed)
                asset.Format = TextureFormat.TrueColor;

            // create the registry containing the sprite assets texture index association
            SpriteToTextureKey = new Dictionary<TImageInfo, string>();

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
                        SpriteToTextureKey[spriteAsset] = ImageGroupAsset.BuildTextureUrl(urlInStorage, i);
                    
                    // texture asset does not need to be generated if using texture atlas
                    if(asset.AtlasPackingEnabled) 
                        continue;

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

        protected readonly Dictionary<TImageInfo, string> ImageToTextureKey;

        protected ImageGroupCommand(string url, ImageGroupParameters<TGroupAsset> asset, Dictionary<TImageInfo, string> imageToTextureKey, bool useSeparateAlphaTexture)
            : base(url, asset)
        {
            ImageToTextureKey = imageToTextureKey;
            UseSeparateAlphaTexture = useSeparateAlphaTexture;
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            if (asset.GroupAsset.AtlasPackingEnabled) 
                yield break;

            foreach (var textureSource in ImageToTextureKey.Values.Distinct())
            {
                if (UseSeparateAlphaTexture)
                {
                    yield return new ObjectUrl(UrlType.Internal, TextureAlphaComponentSplitter.GenerateColorTextureURL(textureSource));
                    yield return new ObjectUrl(UrlType.Internal, TextureAlphaComponentSplitter.GenerateAlphaTextureURL(textureSource));
                }
                else
                {
                    yield return new ObjectUrl(UrlType.Internal, textureSource);
                }
            }
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            var imageGroupData = new TImageGroupData { Images = new List<TImageData>() };

            // Create atlas texture
            var regionDictionary = new Dictionary<TImageInfo, Tuple<int, RotatableRectangle>>();

            // Generate texture atlas
            if (asset.GroupAsset.AtlasPackingEnabled)
            {
                var resultStatus = CreateAndSaveTextureAtlasImage(commandContext.Logger, ref regionDictionary);

                if (resultStatus != ResultStatus.Successful) 
                    return Task.FromResult(resultStatus);
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
                if (asset.GroupAsset.AtlasPackingEnabled)
                {
                    var regionData = regionDictionary[image];
                    var region = regionData.Item2;

                    var imageRegion = new Rectangle(asset.GroupAsset.AtlasBorderSize + region.Value.X, asset.GroupAsset.AtlasBorderSize + region.Value.Y,
                        region.Value.Width - 2 * asset.GroupAsset.AtlasBorderSize, region.Value.Height - 2 * asset.GroupAsset.AtlasBorderSize);

                    newImage.Region = imageRegion;

                    newImage.Orientation = (region.IsRotated) ? ImageOrientation.Rotated90 : ImageOrientation.AsIs;

                    // Auto assign Width, and height to image
                    image.TextureRegion.Width = imageRegion.Width;
                    image.TextureRegion.Height = imageRegion.Height;
                }
                else
                {
                    newImage.Region = image.TextureRegion;
                    newImage.Orientation = image.Orientation;
                }

                if (UseSeparateAlphaTexture)
                {
                    var baseLocation = (asset.GroupAsset.AtlasPackingEnabled)
                        ? ImageGroupAsset.BuildTextureAtlasUrl(Url, regionDictionary[image].Item1) 
                        : ImageToTextureKey[image];

                    newImage.Texture = new ContentReference<Texture2D> { Location = TextureAlphaComponentSplitter.GenerateColorTextureURL(baseLocation) };
                    newImage.TextureAlpha = new ContentReference<Texture2D> { Location = TextureAlphaComponentSplitter.GenerateAlphaTextureURL(baseLocation) };
                }
                else
                {
                    newImage.Texture = new ContentReference<Texture2D> { Location = (asset.GroupAsset.AtlasPackingEnabled)
                        ? ImageGroupAsset.BuildTextureAtlasUrl(Url, regionDictionary[image].Item1) 
                        : ImageToTextureKey[image] };
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
        /// <param name="logger">Status Logger</param>
        /// <param name="regionDictionary">Output that contains Key for each image and a tuple linking the image with a texture atlas index and its region</param>
        /// <returns>Status of building</returns>
        private ResultStatus CreateAndSaveTextureAtlasImage(Logger logger, 
            ref Dictionary<TImageInfo, Tuple<int, RotatableRectangle>> regionDictionary)
        {
            // Pack textures
            using (var texTool = new TextureTool())
            {
                var textureElements = new Dictionary<string, IntermediateTexture>();

                // Input textures
                var imageDictionary = new Dictionary<string, Image>();
                var imageInfoDictionary = new Dictionary<string, TImageInfo>();

                for(var i = 0 ; i < asset.GroupAsset.Images.Count ; ++i)
                {
                    var image = asset.GroupAsset.Images[i];

                    // Lazy load input texture and cache in the dictionary for the later use
                    Image texture;

                    if (!imageDictionary.ContainsKey(ImageToTextureKey[image]))
                    {
                        texture = LoadImage(texTool, new UFile(image.Source));
                        imageDictionary[ImageToTextureKey[image]] = texture;
                    }
                    else
                    {
                        texture = imageDictionary[ImageToTextureKey[image]];
                    }

                    var key = Url + "_" + i;

                    textureElements.Add(
                        key,
                        new IntermediateTexture
                        {
                            Texture = texture,
                            AddressModeU = image.AddressModeU,
                            AddressModeV = image.AddressModeV,
                            BorderColor = image.BorderColor
                        }
                    );

                    imageInfoDictionary[key] = image;
                }

                // Initialize packing configuration from GroupAsset
                var texturePacker = new TexturePacker
                    {
                        Algorithm = asset.GroupAsset.AtlasPackingAlgorithm,
                        UseMultipack = asset.GroupAsset.AtlasUseMultipack,
                        MaxHeight = asset.GroupAsset.AtlasMaxHeight,
                        MaxWidth = asset.GroupAsset.AtlasMaxWidth,
                        UseRotation = asset.GroupAsset.AtlasUseRotation,
                        BorderSize = asset.GroupAsset.AtlasBorderSize,
                        AtlasSizeContraint = AtlasSizeConstraints.PowerOfTwo
                    };

                var canPackAllTextures = texturePacker.PackTextures(textureElements);

                if (!canPackAllTextures)
                {
                    logger.Error("Failed to pack all textures");
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

                        regionDictionary.Add(imageInfoDictionary[textureKey], new Tuple<int, RotatableRectangle>(textureAtlasIndex, texture.Region));
                    }

                    // Dispose used textures
                    foreach (var image in imageDictionary.Values)
                        image.Dispose();

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
                texTool.Decompress(texImage);

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