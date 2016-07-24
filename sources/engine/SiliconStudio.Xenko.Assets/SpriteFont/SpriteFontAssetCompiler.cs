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
            var colorSpace = context.GetColorSpace();

            if (asset.FontType is SignedDistanceFieldSpriteFontType)
            {
                var fontTypeSDF = asset.FontType as SignedDistanceFieldSpriteFontType;

                // copy the asset and transform the source and character set file path to absolute paths
                var assetClone = (SpriteFontAsset)AssetCloner.Clone(asset);
                var assetDirectory = assetAbsolutePath.GetParent();
                assetClone.FontSource = asset.FontSource;
                fontTypeSDF.CharacterSet = !string.IsNullOrEmpty(fontTypeSDF.CharacterSet) ? UPath.Combine(assetDirectory, fontTypeSDF.CharacterSet) : null;

                result.BuildSteps = new AssetBuildStep(AssetItem) { new SignedDistanceFieldFontCommand(urlInStorage, assetClone) };
            }
            else
            if (asset.FontType is RuntimeRasterizedSpriteFontType)
            {
                UFile fontPathOnDisk = asset.FontSource.GetFontPath(result);
                if (fontPathOnDisk == null)
                {
                    result.Error("Runtime rasterized font compilation failed. Font {0} was not found on this machine.", asset.FontSource.GetFontName());
                    result.BuildSteps = new AssetBuildStep(AssetItem) { new FailedFontCommand() };
                    return;
                }

                var fontImportLocation = FontHelper.GetFontPath(asset.FontSource.GetFontName(), asset.FontSource.Style);

                result.BuildSteps = new AssetBuildStep(AssetItem)
                {
                    new ImportStreamCommand { SourcePath = fontPathOnDisk, Location = fontImportLocation },
                    new RuntimeRasterizedFontCommand(urlInStorage, asset)
                };  
            }
            else
            {
                var fontTypeStatic = asset.FontType as OfflineRasterizedSpriteFontType;
                if (fontTypeStatic == null)
                    throw new ArgumentException("Tried to compile a non-offline rasterized sprite font with the compiler for offline resterized fonts!");

                // copy the asset and transform the source and character set file path to absolute paths
                var assetClone = (SpriteFontAsset)AssetCloner.Clone(asset);
                var assetDirectory = assetAbsolutePath.GetParent();
                assetClone.FontSource = asset.FontSource;
                fontTypeStatic.CharacterSet = !string.IsNullOrEmpty(fontTypeStatic.CharacterSet) ? UPath.Combine(assetDirectory, fontTypeStatic.CharacterSet): null;

                result.BuildSteps = new AssetBuildStep(AssetItem) { new OfflineRasterizedFontCommand(urlInStorage, assetClone, colorSpace) };
            }
        }

        internal class OfflineRasterizedFontCommand : AssetCommand<SpriteFontAsset>
        {
            private ColorSpace colorspace;

            public OfflineRasterizedFontCommand(string url, SpriteFontAsset description, ColorSpace colorspace)
                : base(url, description)
            {
                this.colorspace = colorspace;
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                var fontTypeStatic = AssetParameters.FontType as OfflineRasterizedSpriteFontType;
                if (fontTypeStatic == null)
                    throw new ArgumentException("Tried to compile a dynamic sprite font with compiler for signed distance field fonts");

                if (File.Exists(fontTypeStatic.CharacterSet))
                    yield return new ObjectUrl(UrlType.File, fontTypeStatic.CharacterSet);
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
                    staticFont = OfflineRasterizedFontCompiler.Compile(FontDataFactory, AssetParameters, colorspace == ColorSpace.Linear);
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
                var assetManager = new ContentManager();
                assetManager.Save(Url, staticFont);

                // dispose textures allocated by the StaticFontCompiler
                foreach (var texture in staticFont.Textures)
                    texture.Dispose();

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        /// <summary>
        /// Scalable (SDF) font build step
        /// </summary>
        internal class SignedDistanceFieldFontCommand : AssetCommand<SpriteFontAsset>
        {
            public SignedDistanceFieldFontCommand(string url, SpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                var fontTypeSDF = AssetParameters.FontType as SignedDistanceFieldSpriteFontType;
                if (fontTypeSDF == null)
                    throw new ArgumentException("Tried to compile a dynamic sprite font with compiler for signed distance field fonts");

                if (File.Exists(fontTypeSDF.CharacterSet))
                    yield return new ObjectUrl(UrlType.File, fontTypeSDF.CharacterSet);
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                // TODO Add parameter hash codes here
                // writer.Write(colorspace);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // try to import the font from the original bitmap or ttf file
                Graphics.SpriteFont scalableFont;
                try
                {
                    scalableFont = SignedDistanceFieldFontCompiler.Compile(FontDataFactory, AssetParameters);
                }
                catch (FontNotFoundException ex)
                {
                    commandContext.Logger.Error("Font [{0}] was not found on this machine.", ex.FontName);
                    return Task.FromResult(ResultStatus.Failed);
                }

                // check that the font data is valid
                if (scalableFont == null || scalableFont.Textures.Count == 0)
                    return Task.FromResult(ResultStatus.Failed);

                // save the data into the database
                var assetManager = new ContentManager();
                assetManager.Save(Url, scalableFont);

                // dispose textures allocated by the StaticFontCompiler
                foreach (var texture in scalableFont.Textures)
                    texture.Dispose();

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        internal class RuntimeRasterizedFontCommand : AssetCommand<SpriteFontAsset>
        {
            public RuntimeRasterizedFontCommand(string url, SpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var dynamicFont = FontDataFactory.NewDynamic(
                    AssetParameters.FontType.Size, AssetParameters.FontSource.GetFontName(), AssetParameters.FontSource.Style, 
                    AssetParameters.FontType.AntiAlias, useKerning:false, extraSpacing:AssetParameters.Spacing, extraLineSpacing:AssetParameters.LineSpacing, 
                    defaultCharacter:AssetParameters.DefaultCharacter);

                var assetManager = new ContentManager();
                assetManager.Save(Url, dynamicFont);

                return Task.FromResult(ResultStatus.Successful);
            }
        }

        /// <summary>
        /// Proxy command which always fails, called when font is compiled with the wrong assets
        /// </summary>
        internal class FailedFontCommand : AssetCommand<SpriteFontAsset>
        {
            public FailedFontCommand() : base(null, null) { }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                return Task.FromResult(ResultStatus.Failed);
            }
        }
    }
}
