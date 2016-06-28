// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Assets.Textures.Packing;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Xenko.Assets.Sprite
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
            var renderingSettings = gameSettingsAsset.Get<RenderingSettings>(context.Platform);

            result.BuildSteps = new ListBuildStep();
            
            // create the registry containing the sprite assets texture index association
            var imageToTextureUrl = new Dictionary<SpriteInfo, string>();

            var colorSpace = context.GetColorSpace();

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

                    var textureUrl = SpriteSheetAsset.BuildTextureUrl(urlInStorage, i);

                    var spriteAssetArray = spriteByTextures[i].ToArray();
                    foreach (var spriteAsset in spriteAssetArray)
                        imageToTextureUrl[spriteAsset] = textureUrl;

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
                        ColorSpace = asset.ColorSpace,
                        Hint = TextureHint.Color
                    };

                    // Get absolute path of asset source on disk
                    var assetDirectory = assetAbsolutePath.GetParent();
                    var assetSource = UPath.Combine(assetDirectory, spriteAssetArray[0].Source);

                    // add the texture build command.
                    result.BuildSteps.Add(new AssetBuildStep(new AssetItem(textureUrl, textureAsset))
                    {
                        new TextureAssetCompiler.TextureConvertCommand(
                            textureUrl,
                            new TextureConvertParameters(assetSource, textureAsset, context.Platform, context.GetGraphicsPlatform(AssetItem.Package), renderingSettings.DefaultGraphicsProfile, gameSettingsAsset.Get<TextureSettings>().TextureQuality, colorSpace))
                    });
                }
            }

            if (!result.HasErrors)
            {
                var parameters = new SpriteSheetParameters(asset, imageToTextureUrl, context.Platform, context.GetGraphicsPlatform(AssetItem.Package), renderingSettings.DefaultGraphicsProfile, gameSettingsAsset.Get<TextureSettings>().TextureQuality, colorSpace);
                result.BuildSteps.Add(new AssetBuildStep(AssetItem) { new SpriteSheetCommand(urlInStorage, parameters) });
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

            /// <inheritdoc/>
            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                foreach (var dependency in AssetParameters.ImageToTextureUrl)
                {
                    // Use UrlType.Content instead of UrlType.Link, as we are actualy using the content linked of assets in order to create the spritesheet
                    yield return new ObjectUrl(UrlType.Content, dependency.Value);
                }
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

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

                var imageGroupData = new SpriteSheet();

                // add the sprite data to the sprite list.
                foreach (var image in AssetParameters.SheetAsset.Sprites)
                {
                    string textureUrl;
                    RectangleF region;
                    ImageOrientation orientation;

                    var borders = image.Borders;
                    var center = image.Center + (image.CenterFromMiddle ? new Vector2(image.TextureRegion.Width, image.TextureRegion.Height) / 2 : Vector2.Zero);

                    if (isPacking
                        && spriteToPackedSprite.ContainsKey(image)) // ensure that unpackable elements (invalid because of null size/texture) are properly added in the sheet using the normal path
                    {
                        var packedSprite = spriteToPackedSprite[image];
                        var isOriginalSpriteRotated = image.Orientation == ImageOrientation.Rotated90;

                        region = packedSprite.Region;
                        orientation = (packedSprite.IsRotated ^ isOriginalSpriteRotated) ? ImageOrientation.Rotated90 : ImageOrientation.AsIs;
                        textureUrl = SpriteSheetAsset.BuildTextureAtlasUrl(Url, spriteToPackedSprite[image].AtlasTextureIndex);

                        // update the center and border info, if the packer rotated the sprite 
                        // note: X->Left, Y->Top, Z->Right, W->Bottom.
                        if (packedSprite.IsRotated)
                        {
                            // turned the sprite CCW
                            if (isOriginalSpriteRotated)
                            {
                                var oldCenterX = center.X;
                                center.X = center.Y;
                                center.Y = region.Height - oldCenterX;

                                var oldBorderW = borders.W;
                                borders.W = borders.X;
                                borders.X = borders.Y;
                                borders.Y = borders.Z;
                                borders.Z = oldBorderW;
                            }
                            else // turned the sprite CW
                            {
                                var oldCenterX = center.X;
                                center.X = region.Width - center.Y;
                                center.Y = oldCenterX;

                                var oldBorderW = borders.W;
                                borders.W = borders.Z;
                                borders.Z = borders.Y;
                                borders.Y = borders.X;
                                borders.X = oldBorderW;
                            }
                        }
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
                        texture = AttachedReferenceManager.CreateProxyObject<Texture>(Guid.Empty, textureUrl);
                    }
                    else
                    {
                        commandContext.Logger.Warning("Image '{0}' has an invalid image source file '{1}', resulting texture will be null.", image.Name, image.Source);
                    }

                    imageGroupData.Sprites.Add(new Graphics.Sprite
                    {
                        Name = image.Name,
                        Region = region,
                        Orientation = orientation,
                        Center = center,
                        Borders = borders,
                        PixelsPerUnit = new Vector2(image.PixelsPerUnit),
                        Texture = texture,
                        IsTransparent = false,
                    });
                }

                // set the transparency information to all the sprites
                if(AssetParameters.SheetAsset.Alpha != AlphaFormat.None) // Skip the calculation when format is forced without alpha.
                {
                    var urlToTexImage = new Dictionary<string, Tuple<TexImage, Image>>();
                    using (var texTool = new TextureTool())
                    {
                        foreach (var sprite in imageGroupData.Sprites)
                        {
                            if (sprite.Texture == null) // the sprite texture is invalid
                                continue;

                            var textureUrl = AttachedReferenceManager.GetOrCreateAttachedReference(sprite.Texture).Url;
                            if (!urlToTexImage.ContainsKey(textureUrl))
                            {
                                var image = assetManager.Load<Image>(textureUrl);
                                var newTexImage = texTool.Load(image, false);// the sRGB mode does not impact on the alpha level
                                texTool.Decompress(newTexImage, false);// the sRGB mode does not impact on the alpha level
                                urlToTexImage[textureUrl] = Tuple.Create(newTexImage, image);
                            }
                            var texImage = urlToTexImage[textureUrl].Item1;

                            var region = new Rectangle
                            {
                                X = (int)Math.Floor(sprite.Region.X),
                                Y = (int)Math.Floor(sprite.Region.Y)
                            };
                            region.Width = (int)Math.Ceiling(sprite.Region.Right) - region.X;
                            region.Height = (int)Math.Ceiling(sprite.Region.Bottom) - region.Y;

                            var alphaLevel = texTool.GetAlphaLevels(texImage, region, null, commandContext.Logger); // ignore transparent color key here because the input image has already been processed
                            sprite.IsTransparent = alphaLevel != AlphaLevels.NoAlpha; 
                        }

                        // free all the allocated images
                        foreach (var tuple in urlToTexImage.Values)
                        {
                            tuple.Item1.Dispose();
                            assetManager.Unload(tuple.Item2);
                        }
                    }
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
                    var textureElements = new List<AtlasTextureElement>();

                    // Input textures
                    var imageDictionary = new Dictionary<string, Image>();
                    var imageInfoDictionary = new Dictionary<string, SpriteInfo>();

                    var sprites = AssetParameters.SheetAsset.Sprites;
                    var packingParameters = AssetParameters.SheetAsset.Packing;
                    bool isSRgb = AssetParameters.SheetAsset.ColorSpace.ToColorSpace(AssetParameters.ColorSpace, TextureHint.Color) == ColorSpace.Linear;

                    for (var i = 0; i < sprites.Count; ++i)
                    {
                        var sprite = sprites[i];
                        if (sprite.TextureRegion.Height == 0 || sprite.TextureRegion.Width == 0 || sprite.Source == null)
                            continue;

                        // Lazy load input texture and cache in the dictionary for the later use
                        Image texture;

                        if (!imageDictionary.ContainsKey(sprite.Source))
                        {
                            texture = LoadImage(texTool, new UFile(sprite.Source), isSRgb);
                            imageDictionary[sprite.Source] = texture;
                        }
                        else
                        {
                            texture = imageDictionary[sprite.Source];
                        }

                        var key = Url + "_" + i;

                        var sourceRectangle = new RotableRectangle(sprite.TextureRegion, sprite.Orientation == ImageOrientation.Rotated90);
                        textureElements.Add(new AtlasTextureElement(key, texture, sourceRectangle, packingParameters.BorderSize, sprite.BorderModeU, sprite.BorderModeV, sprite.BorderColor));

                        imageInfoDictionary[key] = sprite;
                    }

                    // find the maximum texture size supported
                    var maximumSize = TextureHelper.FindMaximumTextureSize(new TextureHelper.ImportParameters(AssetParameters), new Size2(int.MaxValue/2, int.MaxValue/2));

                    // Initialize packing configuration from GroupAsset
                    var texturePacker = new TexturePacker
                    {
                        Algorithm = packingParameters.PackingAlgorithm,
                        AllowMultipack = packingParameters.AllowMultipacking,
                        MaxWidth = maximumSize.Width,
                        MaxHeight = maximumSize.Height,
                        AllowRotation = packingParameters.AllowRotations,
                    };

                    var canPackAllTextures = texturePacker.PackTextures(textureElements);

                    if (!canPackAllTextures)
                    {
                        logger.Error("Failed to pack all textures");
                        return ResultStatus.Failed;
                    }

                    // Create and save every generated texture atlas
                    for (var textureAtlasIndex = 0; textureAtlasIndex < texturePacker.AtlasTextureLayouts.Count; ++textureAtlasIndex)
                    {
                        var atlasLayout = texturePacker.AtlasTextureLayouts[textureAtlasIndex];

                        ResultStatus resultStatus;
                        using (var atlasImage = AtlasTextureFactory.CreateTextureAtlas(atlasLayout, isSRgb))
                        using (var texImage = texTool.Load(atlasImage, isSRgb))
                        {
                            var outputUrl = SpriteSheetAsset.BuildTextureAtlasUrl(Url, textureAtlasIndex);
                            var convertParameters = new TextureHelper.ImportParameters(AssetParameters) { OutputUrl = outputUrl };
                            resultStatus = TextureHelper.ImportTextureImage(texTool, texImage, convertParameters, CancellationToken, logger);
                        }

                        foreach (var texture in atlasLayout.Textures)
                            spriteToPackedSprite.Add(imageInfoDictionary[texture.Name], new PackedSpriteInfo(texture.DestinationRegion, textureAtlasIndex, packingParameters.BorderSize));

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

                    if (texImage.Format == PixelFormat.B8G8R8A8_UNorm || texImage.Format == PixelFormat.B8G8R8A8_UNorm_SRgb)
                        texTool.SwitchChannel(texImage);

                    return texTool.ConvertToXenkoImage(texImage);
                }
            }

            private class PackedSpriteInfo
            {
                private RotableRectangle packedRectangle;
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
                            borderSize + packedRectangle.X, 
                            borderSize + packedRectangle.Y,
                            packedRectangle.Width - 2 * borderSize,
                            packedRectangle.Height - 2 * borderSize);
                    }
                }

                /// <summary>
                /// Indicate if the packed sprite have been rotated.
                /// </summary>
                public bool IsRotated { get { return packedRectangle.IsRotated; } }

                public PackedSpriteInfo(RotableRectangle packedRectangle, int atlasTextureIndex, float borderSize)
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
                PlatformType platform, GraphicsPlatform graphicsPlatform, GraphicsProfile graphicsProfile, TextureQuality textureQuality, ColorSpace colorSpace)
            {
                ImageToTextureUrl = imageToTextureUrl;
                SheetAsset = sheetAsset;
                Platform = platform;
                GraphicsPlatform = graphicsPlatform;
                GraphicsProfile = graphicsProfile;
                TextureQuality = textureQuality;
                ColorSpace = colorSpace;
            }

            public SpriteSheetAsset SheetAsset;

            public PlatformType Platform;

            public GraphicsPlatform GraphicsPlatform;

            public GraphicsProfile GraphicsProfile;

            public TextureQuality TextureQuality;

            public ColorSpace ColorSpace;

            public Dictionary<SpriteInfo, string> ImageToTextureUrl { get; set; }
        } 
    }
}
