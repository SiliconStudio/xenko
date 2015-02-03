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
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;

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
            SeparateAlphaTexture = TextureCommandHelper.ShouldSeparateAlpha(asset.Alpha, asset.Format, context.Platform, context.GetGraphicsProfile());

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

            // add the sprite data to the sprite list.
            foreach (var uiImage in asset.GroupAsset.Images)
            {
                var newImage = new TImageData
                {
                    Name = uiImage.Name,
                    Region = uiImage.TextureRegion,
                    IsTransparent = asset.GroupAsset.Alpha != AlphaFormat.None, // todo analyze texture region texture data to auto-determine alpha?
                    Orientation = uiImage.Orientation,
                };

                if (UseSeparateAlphaTexture)
                {
                    var baseLocation = ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[uiImage]);
                    newImage.Texture = new ContentReference<Graphics.Texture> { Location = TextureAlphaComponentSplitter.GenerateColorTextureURL(baseLocation) };
                    newImage.TextureAlpha = new ContentReference<Graphics.Texture> { Location = TextureAlphaComponentSplitter.GenerateAlphaTextureURL(baseLocation) };
                }
                else
                {
                    newImage.Texture = new ContentReference<Graphics.Texture> { Location = ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[uiImage]) };
                }

                SetImageSpecificFields(uiImage, newImage);

                imageGroupData.Images.Add(newImage);
            }

            // save the imageData into the data base
            assetManager.Save(Url, imageGroupData);

            return Task.FromResult(ResultStatus.Successful);
        }

        protected virtual void SetImageSpecificFields(TImageInfo imageInfo, TImageData newImage)
        {
        }
    }
    
    /// <summary>
    /// SharedParameters used for converting/processing the texture in the storage.
    /// </summary>
    [DataContract]
    public class ImageGroupParameters<T>
    {
        public ImageGroupParameters()
        {
        }
    
        public ImageGroupParameters(T groupAsset, PlatformType platform)
        {
            GroupAsset = groupAsset;
            Platform = platform;
        }

        public T GroupAsset;
    
        public PlatformType Platform;
    }
}