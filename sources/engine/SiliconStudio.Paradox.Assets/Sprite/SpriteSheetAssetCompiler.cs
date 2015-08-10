// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Assets.Textures.Packing;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.TextureConverter;

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
            var gameSettingsAsset = context.GetGameSettingsAsset();

            result.BuildSteps = new AssetBuildStep(AssetItem);
            
            // create the registry containing the sprite assets texture index association
            var imageToTextureUrl = new Dictionary<SpriteInfo, string>();

            // create and add import texture commands
            if (asset.Sprites != null && !asset.Packing.Enabled)
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
                        imageToTextureUrl[spriteAsset] = SpriteSheetAsset.BuildTextureUrl(urlInStorage, i);

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
                            new TextureConvertParameters(assetSource, textureAsset, context.Platform, context.GetGraphicsPlatform(), gameSettingsAsset.DefaultGraphicsProfile, gameSettingsAsset.TextureQuality)));
                }

                result.BuildSteps.Add(new WaitBuildStep()); // wait the textures to be imported
            }

            if (!result.HasErrors)
            {
                var parameters = new SpriteSheetParameters(asset, imageToTextureUrl, context.Platform, context.GetGraphicsPlatform(), gameSettingsAsset.DefaultGraphicsProfile, gameSettingsAsset.TextureQuality);
                result.BuildSteps.Add(new SpriteSheetCommand(urlInStorage, parameters));                
            }
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class SpriteSheetCommand : AssetCommand<SpriteSheetParameters>
        {
            public SpriteSheetCommand(string url, SpriteSheetParameters assetParameters)
                : base(url, assetParameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new AssetManager();

                // Create atlas texture
                Dictionary<SpriteInfo, PackedSpriteInfo> spriteToPackedSprite = null;

                // Generate texture atlas
                var isPacking = AssetParameters.SheetAsset.Packing.Enabled;
                if (isPacking)
                {
                    var resultStatus = CreateAtlasTextures(commandContext.Logger, out spriteToPackedSprite);

                    if (resultStatus != ResultStatus.Successful)
                        return Task.FromResult(resultStatus);
                }

                var imageGroupData = new SpriteSheet { Sprites = new List<Graphics.Sprite>() };

                // add the sprite data to the sprite list.
                foreach (var image in AssetParameters.SheetAsset.Sprites)
                {
                    RectangleF region;
                    ImageOrientation orientation;
                    string textureUrl;

                    if (isPacking)
                    {
                        if (!spriteToPackedSprite.ContainsKey(image)) 
                            continue;

                        var packedSprite = spriteToPackedSprite[image];
                        var isOriginalSpriteRotated = image.Orientation == ImageOrientation.Rotated90;

                        region = packedSprite.Region;
                        orientation = (packedSprite.IsRotated ^ isOriginalSpriteRotated) ? ImageOrientation.Rotated90 : ImageOrientation.AsIs;
                        textureUrl = SpriteSheetAsset.BuildTextureAtlasUrl(Url, spriteToPackedSprite[image].AtlasTextureIndex);
                    }
                    else
                    {
                        region = image.TextureRegion;
                        orientation = image.Orientation;
                        AssetParameters.ImageToTextureUrl.TryGetValue(image, out textureUrl);
                    }

                    // Affect the texture
                    Texture texture = null;
                    if (textureUrl != null)
                    {
                        texture = AttachedReferenceManager.CreateSerializableVersion<Texture>(Guid.Empty, textureUrl);
                    }
                    else
                    {
                        commandContext.Logger.Warning("Image '{0}' has an invalid image source file '{1}', resulting texture will be null.", image.Name, image.Source);
                    }

                    imageGroupData.Sprites.Add(new Graphics.Sprite
                    {
                        Name = image.Name,
                        Region = region,
                        IsTransparent = AssetParameters.SheetAsset.Alpha != AlphaFormat.None, // todo analyze texture region texture data to auto-determine alpha?
                        Orientation = orientation,
                        Center = image.Center + (image.CenterFromMiddle ? new Vector2(image.TextureRegion.Width, image.TextureRegion.Height) / 2 : Vector2.Zero),
                        Borders = image.Borders,
                        PixelsPerUnit = new Vector2(image.PixelsPerUnit),
                        Texture = texture
                    });
                }

                // save the imageData into the data base
                assetManager.Save(Url, imageGroupData);

                return Task.FromResult(ResultStatus.Successful);
            }

            /// <summary>
            /// Creates and Saves texture atlas image from images in GroupAsset
            /// </summary>
            /// <param name="logger">Status Logger</param>
            /// <param name="spriteToPackedSprite">A map associating the packed sprite info to the original sprite</param>
            /// <returns>Status of building</returns>
            private ResultStatus CreateAtlasTextures(Logger logger, out Dictionary<SpriteInfo, PackedSpriteInfo> spriteToPackedSprite)
            {
                spriteToPackedSprite = new Dictionary<SpriteInfo, PackedSpriteInfo>();

                // Pack textures
                using (var texTool = new TextureTool())
                {
                    var textureElements = new Dictionary<string, IntermediateTexture>();

                    // Input textures
                    var imageDictionary = new Dictionary<string, Image>();
                    var imageInfoDictionary = new Dictionary<string, SpriteInfo>();

                    var canUseRotation = true;

                    var sprites = AssetParameters.SheetAsset.Sprites;
                    var packingParameters = AssetParameters.SheetAsset.Packing;

                    for (var i = 0; i < sprites.Count; ++i)
                    {
                        var sprite = sprites[i];
                        if (sprite.TextureRegion.Height == 0)
                            continue;

                        canUseRotation &= sprite.Orientation == ImageOrientation.AsIs;

                        // Lazy load input texture and cache in the dictionary for the later use
                        Image texture;

                        if (!imageDictionary.ContainsKey(sprite.Source))
                        {
                            texture = LoadImage(texTool, new UFile(sprite.Source), AssetParameters.SheetAsset.SRgb);
                            imageDictionary[sprite.Source] = texture;
                        }
                        else
                        {
                            texture = imageDictionary[sprite.Source];
                        }

                        var key = Url + "_" + i;

                        textureElements.Add(
                            key,
                            new IntermediateTexture
                            {
                                Texture = texture,
                                Region = sprite.TextureRegion,
                                AddressModeU = sprite.BorderModeU,
                                AddressModeV = sprite.BorderModeV,
                                BorderColor = sprite.BorderColor
                            }
                        );

                        imageInfoDictionary[key] = sprite;
                    }

                    // Initialize packing configuration from GroupAsset
                    var texturePacker = new TexturePacker
                    {
                        Algorithm = packingParameters.PackingAlgorithm,
                        UseMultipack = packingParameters.AllowMultipacking,
                        MaxHeight = packingParameters.AtlasMaximumSize.X,
                        MaxWidth = packingParameters.AtlasMaximumSize.Y,
                        UseRotation = canUseRotation && packingParameters.AllowRotations,
                        BorderSize = packingParameters.BorderSize,
                        AtlasSizeContraint = AtlasSizeConstraints.PowerOfTwo
                    };

                    var canPackAllTextures = texturePacker.PackTextures(textureElements);

                    if (!canPackAllTextures)
                    {
                        logger.Error("Failed to pack all textures");
                        return ResultStatus.Failed;
                    }

                    // Create and save every generated texture atlas
                    for (var textureAtlasIndex = 0; textureAtlasIndex < texturePacker.TextureAtlases.Count; ++textureAtlasIndex)
                    {
                        var textureAtlas = texturePacker.TextureAtlases[textureAtlasIndex];

                        ResultStatus resultStatus;
                        using (var atlasImage = TexturePacker.Factory.CreateTextureAtlas(textureAtlas))
                        using (var texImage = texTool.Load(atlasImage))
                        {
                            var outputUrl = SpriteSheetAsset.BuildTextureAtlasUrl(Url, textureAtlasIndex);
                            var convertParameters = new TextureHelper.ImportParameters(AssetParameters) { OutputUrl = outputUrl };
                            resultStatus = TextureHelper.ImportTextureImage(texTool, texImage, convertParameters, CancellationToken, logger);
                        }

                        foreach (var texture in textureAtlas.Textures)
                        {
                            var textureKey = texture.PackingRegion.Key;

                            spriteToPackedSprite.Add(imageInfoDictionary[textureKey], new PackedSpriteInfo(texture.PackingRegion, textureAtlasIndex, packingParameters.BorderSize));
                        }

                        if (resultStatus != ResultStatus.Successful)
                        {
                            // Dispose used textures
                            foreach (var image in imageDictionary.Values)
                                image.Dispose();

                            return resultStatus;
                        }
                    }

                    // Dispose used textures
                    foreach (var image in imageDictionary.Values)
                        image.Dispose();
                }

                return ResultStatus.Successful;
            }

            /// <summary>
            /// Loads image from a path with texTool
            /// </summary>
            /// <param name="texTool">A tool for loading an image</param>
            /// <param name="sourcePath">Source path of an image</param>
            /// <param name="isSRgb">Indicate if the texture to load is sRGB</param>
            /// <returns></returns>
            private static Image LoadImage(TextureTool texTool, UFile sourcePath, bool isSRgb)
            {
                using (var texImage = texTool.Load(sourcePath, isSRgb))
                {
                    texTool.Decompress(texImage, isSRgb);

                    if (texImage.Format == PixelFormat.B8G8R8A8_UNorm)
                        texTool.SwitchChannel(texImage);

                    return texTool.ConvertToParadoxImage(texImage);
                }
            }

            private class PackedSpriteInfo
            {
                private RotatableRectangle packedRectangle;
                private readonly float borderSize;

                /// <summary>
                /// The index of the atlas texture the sprite has been packed in.
                /// </summary>
                public int AtlasTextureIndex { get; private set; }

                /// <summary>
                /// Gets the region of the packed sprite.
                /// </summary>
                public RectangleF Region
                {
                    get
                    {
                        return new RectangleF(
                            borderSize + packedRectangle.Value.X, 
                            borderSize + packedRectangle.Value.Y,
                            packedRectangle.Value.Width - 2 * borderSize,
                            packedRectangle.Value.Height - 2 * borderSize);
                    }
                }

                /// <summary>
                /// Indicate if the packed sprite have been rotated.
                /// </summary>
                public bool IsRotated { get { return packedRectangle.IsRotated; } }

                public PackedSpriteInfo(RotatableRectangle packedRectangle, int atlasTextureIndex, float borderSize)
                {
                    this.packedRectangle = packedRectangle;
                    this.borderSize = borderSize;
                    AtlasTextureIndex = atlasTextureIndex;
                }
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

            public SpriteSheetParameters(SpriteSheetAsset sheetAsset, Dictionary<SpriteInfo, string> imageToTextureUrl, 
                PlatformType platform, GraphicsPlatform graphicsPlatform, GraphicsProfile graphicsProfile, TextureQuality textureQuality)
            {
                ImageToTextureUrl = imageToTextureUrl;
                SheetAsset = sheetAsset;
                Platform = platform;
                GraphicsPlatform = graphicsPlatform;
                GraphicsProfile = graphicsProfile;
                TextureQuality = textureQuality;
            }

            public SpriteSheetAsset SheetAsset;

            public PlatformType Platform;

            public GraphicsPlatform GraphicsPlatform;

            public GraphicsProfile GraphicsProfile;

            public TextureQuality TextureQuality;

            public Dictionary<SpriteInfo, string> ImageToTextureUrl { get; set; }
        } 
    }
}