// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    internal class TextureAssetCompiler : AssetCompilerBase<TextureAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, TextureAsset asset, AssetCompilerResult result)
        {
            if (!EnsureSourceExists(result, asset, assetAbsolutePath))
                return;
        
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetAbsolutePath, asset.Source);

            var parameter = new TextureConvertParameters(assetSource, asset, context.Platform, context.GetGraphicsPlatform(), context.GetGraphicsProfile(), context.GetTextureQuality());
            result.BuildSteps = new AssetBuildStep(AssetItem) { new TextureConvertCommand(urlInStorage, parameter) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        internal class TextureConvertCommand : AssetCommand<TextureConvertParameters>
        {
            public TextureConvertCommand()
            {
            }

            public TextureConvertCommand(string url, TextureConvertParameters description)
                : base(url, description)
            {
            }

            public override System.Collections.Generic.IEnumerable<ObjectUrl> GetInputFiles()
            {
                // TODO dependency not working
                yield return new ObjectUrl(UrlType.File, asset.SourcePathFromDisk);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var texture = asset.Texture;

                var importResult = TextureCommandHelper.ImportAndSaveTextureImage(asset.SourcePathFromDisk, Url, texture, asset, CancellationToken, commandContext.Logger);

                return Task.FromResult(importResult);
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
            TextureQuality textureQuality)
        {
            SourcePathFromDisk = sourcePathFromDisk;
            Texture = texture;
            Platform = platform;
            GraphicsPlatform = graphicsPlatform;
            GraphicsProfile = graphicsProfile;
            TextureQuality = textureQuality;
        }

        public UFile SourcePathFromDisk;

        public TextureAsset Texture;

        public PlatformType Platform;

        public GraphicsPlatform GraphicsPlatform;

        public GraphicsProfile GraphicsProfile;

        public TextureQuality TextureQuality;
    }
}