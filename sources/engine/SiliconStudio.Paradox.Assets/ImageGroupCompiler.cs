using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal abstract class ImageGroupCompiler<TGroupAsset, TImageInfo> : 
        AssetCompilerBase<TGroupAsset> 
        where TGroupAsset : ImageGroupAsset<TImageInfo> 
        where TImageInfo: ImageInfo
    {
        private bool TextureFileIsValid(UFile file)
        {
            return file != null && File.Exists(file);
        }

        protected Dictionary<TImageInfo, int> CompileGroup(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, TGroupAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new AssetBuildStep(AssetItem);
            
            // create the registry containing the sprite assets texture index association
            var imageToTextureIndex = new Dictionary<TImageInfo, int>();

            // create and add import texture commands
            if (asset.Images != null)
            {
                // sort sprites by referenced texture.
                var spriteByTextures = asset.Images.GroupBy(x => x.Source).ToArray();
                for (int i = 0; i < spriteByTextures.Length; i++)
                {
                    // skip the texture if the file is not valid.
                    var textureFile = spriteByTextures[i].Key;
                    if(!TextureFileIsValid(textureFile))
                        continue;

                    var spriteAssetArray = spriteByTextures[i].ToArray();
                    foreach (var spriteAsset in spriteAssetArray)
                        imageToTextureIndex[spriteAsset] = i;

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
                            new TextureConvertParameters(assetSource, textureAsset, context.Platform, context.GetGraphicsPlatform(), context.GetGraphicsProfile(), context.GetTextureQuality())));
                }

                result.BuildSteps.Add(new WaitBuildStep()); // wait the textures to be imported
            }
            return imageToTextureIndex;
        }   
    }

    /// <summary>
    /// Command used to convert the texture in the storage
    /// </summary>
    public class ImageGroupCommand<TGroupAsset, TImageInfo, TImageGroupData, TImageData> : AssetCommand<ImageGroupParameters<TGroupAsset>>
        where TGroupAsset : ImageGroupAsset<TImageInfo>
        where TImageInfo : ImageInfo
        where TImageGroupData : ImageGroup<TImageData>, new()
        where TImageData : ImageFragment, new()
    {
        protected readonly Dictionary<TImageInfo, int> ImageToTextureIndex;

        protected ImageGroupCommand(string url, ImageGroupParameters<TGroupAsset> asset, Dictionary<TImageInfo, int> imageToTextureIndex)
            : base(url, asset)
        {
            ImageToTextureIndex = imageToTextureIndex;
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            for (int i = 0; i < ImageToTextureIndex.Values.Distinct().Count(); i++)
                yield return new ObjectUrl(UrlType.Internal, ImageGroupAsset.BuildTextureUrl(Url, i));
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

                int imageIndex;
                if (ImageToTextureIndex.TryGetValue(uiImage, out imageIndex))
                {
                    newImage.Texture = AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, ImageGroupAsset.BuildTextureUrl(Url, ImageToTextureIndex[uiImage]));
                }
                else
                {
                    commandContext.Logger.Warning("Image '{0}' has an invalid image source file '{1}', resulting texture will be null.", uiImage.Name, uiImage.Source);
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