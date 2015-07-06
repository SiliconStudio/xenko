using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// The <see cref="SpriteSheetAsset"/> compiler.
    /// </summary>
    public class SpriteSheetAssetCompiler : AssetCompilerBase<SpriteSheetAsset> 
    {
        private bool TextureFileIsValid(UFile file)
        {
            return file != null && File.Exists(file);
        }

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteSheetAsset asset, AssetCompilerResult result)
        {
            result.BuildSteps = new AssetBuildStep(AssetItem);
            
            // create the registry containing the sprite assets texture index association
            var imageToTextureIndex = new Dictionary<SpriteInfo, int>();

            // create and add import texture commands
            if (asset.Sprites != null)
            {
                // sort sprites by referenced texture.
                var spriteByTextures = asset.Sprites.GroupBy(x => x.Source).ToArray();
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
                            SpriteSheetAsset.BuildTextureUrl(urlInStorage, i),
                            new TextureConvertParameters(assetSource, textureAsset, context.Platform, context.GetGraphicsPlatform(), context.GetGraphicsProfile(), context.GetTextureQuality())));
                }

                result.BuildSteps.Add(new WaitBuildStep()); // wait the textures to be imported
            }

            if (!result.HasErrors)
                result.BuildSteps.Add(new SpriteSheetCommand(urlInStorage, new SpriteSheetParameters(asset, context.Platform), imageToTextureIndex));
        }   
    }

    /// <summary>
    /// Command used to convert the texture in the storage
    /// </summary>
    public class SpriteSheetCommand : AssetCommand<SpriteSheetParameters>
    {
        protected readonly Dictionary<SpriteInfo, int> ImageToTextureIndex;

        public SpriteSheetCommand(string url, SpriteSheetParameters assetParameters, Dictionary<SpriteInfo, int> imageToTextureIndex)
            : base(url, assetParameters)
        {
            ImageToTextureIndex = imageToTextureIndex;
        }

        public override IEnumerable<ObjectUrl> GetInputFiles()
        {
            for (int i = 0; i < ImageToTextureIndex.Values.Distinct().Count(); i++)
                yield return new ObjectUrl(UrlType.Internal, SpriteSheetAsset.BuildTextureUrl(Url, i));
        }

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            var assetManager = new AssetManager();

            var imageGroupData = new SpriteSheet { Sprites = new List<Graphics.Sprite>() };

            // add the sprite data to the sprite list.
            foreach (var image in AssetParameters.SheetAsset.Sprites)
            {
                var newImage = new Graphics.Sprite
                {
                    Name = image.Name,
                    Region = image.TextureRegion,
                    IsTransparent = AssetParameters.SheetAsset.Alpha != AlphaFormat.None, // todo analyze texture region texture data to auto-determine alpha?
                    Orientation = image.Orientation,
                    Center = image.Center + (image.CenterFromMiddle ? new Vector2(image.TextureRegion.Width, image.TextureRegion.Height) / 2 : Vector2.Zero),
                    Borders = image.Borders,
                };

                int imageIndex;
                if (ImageToTextureIndex.TryGetValue(image, out imageIndex))
                {
                    newImage.Texture = AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, SpriteSheetAsset.BuildTextureUrl(Url, ImageToTextureIndex[image]));
                }
                else
                {
                    commandContext.Logger.Warning("Image '{0}' has an invalid image source file '{1}', resulting texture will be null.", image.Name, image.Source);
                }

                imageGroupData.Sprites.Add(newImage);
            }

            // save the imageData into the data base
            assetManager.Save(Url, imageGroupData);

            return Task.FromResult(ResultStatus.Successful);
        }
    }

    /// <summary>
    /// SharedParameters used for converting/processing the texture in the storage.
    /// </summary>
    [DataContract]
    public class SpriteSheetParameters
    {
        public SpriteSheetParameters()
        {
        }

        public SpriteSheetParameters(SpriteSheetAsset sheetAsset, PlatformType platform)
        {
            SheetAsset = sheetAsset;
            Platform = platform;
        }

        public SpriteSheetAsset SheetAsset;

        public PlatformType Platform;
    }
}