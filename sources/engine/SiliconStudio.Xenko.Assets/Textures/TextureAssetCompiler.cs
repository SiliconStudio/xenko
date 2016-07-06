// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    public class TextureAssetCompiler : AssetCompilerBase<TextureAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, TextureAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourcesExist(result, asset, assetAbsolutePath))
                return;
        
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var colorSpace = context.GetColorSpace();

            var parameter = new TextureConvertParameters(assetSource, asset, context.Platform, context.GetGraphicsPlatform(AssetItem.Package), gameSettingsAsset.Get<RenderingSettings>(context.Platform).DefaultGraphicsProfile, gameSettingsAsset.Get<TextureSettings>().TextureQuality, colorSpace);
            result.BuildSteps = new AssetBuildStep(AssetItem) { new TextureConvertCommand(urlInStorage, parameter) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class TextureConvertCommand : AssetCommand<TextureConvertParameters>
        {
            public TextureConvertCommand(string url, TextureConvertParameters description)
                : base(url, description)
            {
            }

            protected override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                // TODO dependency not working
                yield return new ObjectUrl(UrlType.File, AssetParameters.SourcePathFromDisk);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var convertParameters = new TextureHelper.ImportParameters(AssetParameters) { OutputUrl = Url };
                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(AssetParameters.SourcePathFromDisk, convertParameters.IsSRgb))
                {
                    var importResult = TextureHelper.ImportTextureImage(texTool, texImage, convertParameters, CancellationToken, commandContext.Logger);

                    return Task.FromResult(importResult);
                }
            }

            protected override void ComputeAssemblyHash(BinarySerializationWriter writer)
            {
                writer.Write(DataSerializer.BinaryFormatVersion);

                // Since Image format is quite stable, we want to manually control it's assembly hash here
                writer.Write(1);
            }
        }
    }


    /// <summary>
    /// SharedParameters used for converting/processing the texture in the storage.
    /// </summary>
    [DataContract]
    public class TextureConvertParameters
    {
        public TextureConvertParameters()
        {
        }

        public TextureConvertParameters(
            UFile sourcePathFromDisk, 
            TextureAsset texture, 
            PlatformType platform, 
            GraphicsPlatform graphicsPlatform, 
            GraphicsProfile graphicsProfile, 
            TextureQuality textureQuality,
            ColorSpace colorSpace)
        {
            SourcePathFromDisk = sourcePathFromDisk;
            Texture = texture;
            Platform = platform;
            GraphicsPlatform = graphicsPlatform;
            GraphicsProfile = graphicsProfile;
            TextureQuality = textureQuality;
            ColorSpace = colorSpace;
        }

        public UFile SourcePathFromDisk;

        public TextureAsset Texture;

        public PlatformType Platform;

        public GraphicsPlatform GraphicsPlatform;

        public GraphicsProfile GraphicsProfile;

        public TextureQuality TextureQuality;

        public ColorSpace ColorSpace;
    }
}
