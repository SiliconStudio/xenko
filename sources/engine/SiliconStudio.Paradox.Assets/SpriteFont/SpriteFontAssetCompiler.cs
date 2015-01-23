// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#pragma warning disable 162 // Unreachable code detected (due to useCacheFonts)
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpDX.DirectWrite;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.SpriteFont.Compiler;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Font;

using Font = SharpDX.DirectWrite.Font;
using FontStyle = SiliconStudio.Paradox.Graphics.Font.FontStyle;

namespace SiliconStudio.Paradox.Assets.SpriteFont
{
    public class SpriteFontAssetCompiler : AssetCompilerBase<SpriteFontAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteFontAsset asset, AssetCompilerResult result)
        {
            if (asset.IsDynamic)
            {
                UFile fontPathOnDisk;
                if (asset.Source != null)
                {
                    var assetDirectory = assetAbsolutePath.GetParent();
                    fontPathOnDisk = UPath.Combine(assetDirectory, asset.Source);
                    if (!File.Exists(fontPathOnDisk))
                    {
                        result.Error("The font source '{0}' does not exist on the PC.", asset.FontName);
                        return;
                    }
                    // set the source filename as font name instead of the font family.
                    asset.FontName = fontPathOnDisk.GetFileName();
                }
                else
                {
                    fontPathOnDisk = GetFontPath(asset, result);
                    if (fontPathOnDisk == null)
                    {
                        result.Error("The font named '{0}' could not be located on the PC.", asset.FontName);
                        return;
                    }
                }
                var fontImportLocation = FontHelper.GetFontPath(asset.FontName, asset.Style);

                result.BuildSteps = new ListBuildStep
                {
                    new ImportStreamCommand { SourcePath = fontPathOnDisk, Location = fontImportLocation },
                    new DynamicFontCommand(urlInStorage, asset)
                };  
            }
            else
            {
                // copy the asset and transform the source and character set file path to absolute paths
                var assetClone = (SpriteFontAsset)AssetCloner.Clone(asset);
                var assetDirectory = assetAbsolutePath.GetParent();
                assetClone.Source = asset.Source != null? UPath.Combine(assetDirectory, asset.Source): null;
                assetClone.CharacterSet = asset.CharacterSet != null ? UPath.Combine(assetDirectory, asset.CharacterSet): null;

                result.BuildSteps = new ListBuildStep { new StaticFontCommand(urlInStorage, assetAbsolutePath, assetClone) };
            }
        }

        internal class StaticFontCommand : AssetCommand<SpriteFontAsset>
        {
            private readonly UFile assetAbsolutePath;

            public StaticFontCommand(string url, UFile assetAbsolutePath, SpriteFontAsset description)
                : base(url, description)
            {
                this.assetAbsolutePath = assetAbsolutePath;
            }

            public override IEnumerable<ObjectUrl> GetInputFiles()
            {
                foreach (var inputFile in base.GetInputFiles())
                {
                    yield return inputFile;
                }

                if(File.Exists(asset.CharacterSet))
                    yield return new ObjectUrl(UrlType.File, asset.CharacterSet);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                const bool useCacheFonts = false;

                // compute the path of the cache files
                var cachedImagePath = assetAbsolutePath.GetFileName() + ".CachedImage";
                var cachedFontGlyphs = assetAbsolutePath.GetFileName() + ".CachedGlyphs";

                // try to import the font from the original bitmap or ttf file
                StaticSpriteFontData data;
                try
                {
                    data = FontCompiler.Compile(asset);
                }
                catch (FontNotFoundException ex) 
                {
                    if (!useCacheFonts)
                    {
                        commandContext.Logger.Error("Font [{0}] was not found on this machine.", ex.FontName);
                        return Task.FromResult(ResultStatus.Failed);
                    }
                    else
                    {
                        // If the original fo
                        commandContext.Logger.Warning("Font [{0}] was not found on this machine. Trying to use cached glyphs/image", ex.FontName);
                        if (!File.Exists(cachedFontGlyphs))
                        {
                            commandContext.Logger.Error("Expecting cached glyphs [{0}]", cachedFontGlyphs);
                            return Task.FromResult(ResultStatus.Failed);
                        }

                        if (!File.Exists(cachedImagePath))
                        {
                            commandContext.Logger.Error("Expecting cached image [{0}]", cachedImagePath);
                            return Task.FromResult(ResultStatus.Failed);
                        }

                        // read the cached glyphs
                        using (var glyphStream = File.OpenRead(cachedFontGlyphs))
                            data = BinarySerialization.Read<StaticSpriteFontData>(glyphStream);

                        // read the cached image
                        data.Bitmaps = new[] { new ContentReference<Image>() };
                        using (var imageStream = File.OpenRead(cachedImagePath))
                            data.Bitmaps[0].Value = Image.Load(imageStream);
                    }
                }

                // check that the font data is valid
                if (data == null || data.Bitmaps.Length == 0)
                    return Task.FromResult(ResultStatus.Failed);

                // save the data into the database
                var imageUrl = Url + "__image";
                data.Bitmaps[0].Location = imageUrl;
                var assetManager = new AssetManager();
                assetManager.Save(Url, data);

                var image = data.Bitmaps[0].Value;

                // cache the generated data
                if (useCacheFonts)
                {
                    try
                    {
                        // the image
                        using (var imageStream = File.OpenWrite(cachedImagePath))
                            image.Save(imageStream, ImageFileType.Paradox);

                        // the glyphs
                        data.Bitmaps = null;
                        using (var glyphStream = File.OpenWrite(cachedFontGlyphs))
                            BinarySerialization.Write(glyphStream, data);
                    }
                    catch (IOException ex)
                    {
                        commandContext.Logger.Warning("Cannot save cached glyphs [{0}] or image [{1}]", ex, cachedFontGlyphs, cachedImagePath);
                    }
                }

                // free the objects
                image.Dispose();

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        private static string GetFontPath(SpriteFontAsset asset, AssetCompilerResult result)
        {
            using (var factory = new Factory())
            {
                Font font;

                using (var fontCollection = factory.GetSystemFontCollection(false))
                {
                    int index;
                    if (!fontCollection.FindFamilyName(asset.FontName, out index))
                    {
                        result.Error("Can't find font '{0}'.", asset.FontName);
                        return null;
                    }

                    using (var fontFamily = fontCollection.GetFontFamily(index))
                    {
                        var weight = asset.Style.IsBold() ? FontWeight.Bold : FontWeight.Regular;
                        var style = asset.Style.IsItalic() ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal;
                        font = fontFamily.GetFirstMatchingFont(weight, FontStretch.Normal, style);
                        if (font == null)
                        {
                            result.Error("Cannot find style '{0}' for font family {1}.", asset.Style, asset.FontName);
                            return null;
                        }
                    }
                }

                var fontFace = new FontFace(font);

                // get the font path on the hard drive
                var file = fontFace.GetFiles().First();
                var referenceKey = file.GetReferenceKey();
                var originalLoader = (FontFileLoaderNative)file.Loader;
                var loader = originalLoader.QueryInterface<LocalFontFileLoader>();
                return loader.GetFilePath(referenceKey);
            }
        }

        internal class DynamicFontCommand : AssetCommand<SpriteFontAsset>
        {
            public DynamicFontCommand(string url, SpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var dynamicFont = new DynamicSpriteFontData
                {
                    Size = FontHelper.PointsToPixels(asset.Size),
                    DefaultCharacter = asset.DefaultCharacter,
                    FontName = asset.FontName,
                    ExtraLineSpacing = asset.LineSpacing,
                    DefaultSize = asset.Size,
                    ExtraSpacing = asset.Spacing,
                    Style = asset.Style,
                    UseKerning = asset.UseKerning,
                    AntiAlias = asset.AntiAlias,
                };

                var assetManager = new AssetManager();
                assetManager.Save(Url, dynamicFont);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
