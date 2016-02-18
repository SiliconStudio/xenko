// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#pragma warning disable 162 // Unreachable code detected (due to useCacheFonts)
using System;
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
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;

using Font = SharpDX.DirectWrite.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class SpriteFontAssetCompiler : AssetCompilerBase<SpriteFontAsset>
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SpriteFontAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;

            var colorSpace = context.GetColorSpace();

            if (asset.IsDynamic)
            {
                UFile fontPathOnDisk;

                if (!string.IsNullOrEmpty(asset.Source))
                {
                    try
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
                    catch (Exception e)
                    {
                        result.Error("The source '{0}' for font '{1}' is not a valid path: {2}", e, asset.Source, asset.FontName, e.Message);
                        return;
                    }
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

                result.BuildSteps = new AssetBuildStep(AssetItem)
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

                try
                {
                    assetClone.Source = !string.IsNullOrEmpty(asset.Source) ? UPath.Combine(assetDirectory, asset.Source) : null;
                }
                catch (Exception e)
                {
                    result.Error("The source '{0}' for font '{1}' is not a valid path: {2}", e, asset.Source, asset.FontName, e.Message);
                    return;
                }


                try
                {
                    assetClone.CharacterSet = !string.IsNullOrEmpty(asset.CharacterSet) ? UPath.Combine(assetDirectory, asset.CharacterSet) : null;
                }
                catch (Exception e)
                {
                    result.Error("The character set '{0}' for font '{1}' is not a valid path: {2}", e, asset.CharacterSet, asset.FontName, e.Message);
                    return;
                }

                result.BuildSteps = new AssetBuildStep(AssetItem) { new StaticFontCommand(urlInStorage, assetClone, colorSpace) };
            }
        }

        internal class StaticFontCommand : AssetCommand<SpriteFontAsset>
        {
            private ColorSpace colorspace;

            public StaticFontCommand(string url, SpriteFontAsset description, ColorSpace colorspace)
                : base(url, description)
            {
                this.colorspace = colorspace;
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                if(File.Exists(AssetParameters.CharacterSet))
                    yield return new ObjectUrl(UrlType.File, AssetParameters.CharacterSet);
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                writer.Write(colorspace);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // try to import the font from the original bitmap or ttf file
                Graphics.SpriteFont staticFont;
                try
                {
                    staticFont = StaticFontCompiler.Compile(FontDataFactory, AssetParameters, colorspace == ColorSpace.Linear);
                }
                catch (FontNotFoundException ex) 
                {
                    commandContext.Logger.Error("Font [{0}] was not found on this machine.", ex.FontName);
                    return Task.FromResult(ResultStatus.Failed);
                }

                // check that the font data is valid
                if (staticFont == null || staticFont.Textures.Count == 0)
                    return Task.FromResult(ResultStatus.Failed);

                // save the data into the database
                var assetManager = new AssetManager();
                assetManager.Save(Url, staticFont);

                // dispose textures allocated by the StaticFontCompiler
                foreach (var texture in staticFont.Textures)
                    texture.Dispose();

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
                var dynamicFont = FontDataFactory.NewDynamic(
                    FontHelper.PointsToPixels(AssetParameters.Size), AssetParameters.FontName, AssetParameters.Style, 
                    AssetParameters.AntiAlias, AssetParameters.UseKerning, AssetParameters.Spacing, AssetParameters.LineSpacing, AssetParameters.DefaultCharacter);

                var assetManager = new AssetManager();
                assetManager.Save(Url, dynamicFont);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
