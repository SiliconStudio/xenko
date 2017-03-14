// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    public class TextureAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(GameSettingsAsset), BuildDependencyType.CompileAsset);
        }

        protected override void Prepare(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (TextureAsset)assetItem.Asset;
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetItem, asset.Source);

            var gameSettingsAsset = context.GetGameSettingsAsset();
            var colorSpace = context.GetColorSpace();

            var parameter = new TextureConvertParameters(assetSource, asset, context.Platform, context.GetGraphicsPlatform(assetItem.Package), gameSettingsAsset.Get<RenderingSettings>(context.Platform).DefaultGraphicsProfile, gameSettingsAsset.Get<TextureSettings>().TextureQuality, colorSpace);
            result.BuildSteps = new AssetBuildStep(assetItem) { new TextureConvertCommand(targetUrlInStorage, parameter, assetItem.Package) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class TextureConvertCommand : AssetCommand<TextureConvertParameters>
        {
            public TextureConvertCommand(string url, TextureConvertParameters description, Package package)
                : base(url, description, package)
            {
            }

            protected override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                // TODO dependency not working
                yield return new ObjectUrl(UrlType.File, Parameters.SourcePathFromDisk);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var convertParameters = new TextureHelper.ImportParameters(Parameters) { OutputUrl = Url };
                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(Parameters.SourcePathFromDisk, convertParameters.IsSRgb))
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
