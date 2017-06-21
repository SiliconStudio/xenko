// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.TextureConverter;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Streaming;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// Texture asset compiler.
    /// </summary>
    [AssetCompiler(typeof(TextureAsset), typeof(AssetCompilationContext))]
    public class TextureAssetCompiler : AssetCompilerBase
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetItem assetItem)
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

            var parameter = new TextureConvertParameters(assetSource, asset, context.Platform, context.GetGraphicsPlatform(assetItem.Package), gameSettingsAsset.GetOrCreate<RenderingSettings>(context.Platform).DefaultGraphicsProfile, gameSettingsAsset.GetOrCreate<TextureSettings>().TextureQuality, colorSpace);
            result.BuildSteps = new AssetBuildStep(assetItem) { new TextureConvertCommand(targetUrlInStorage, parameter, assetItem.Package) };
        }

        /// <summary>
        /// Command used to convert the texture in the storage
        /// </summary>
        public class TextureConvertCommand : AssetCommand<TextureConvertParameters>
        {
            private readonly TagSymbol disableCompressionSymbol;

            public TextureConvertCommand(string url, TextureConvertParameters description, Package package)
                : base(url, description, package)
            {
                InputFilesGetter = GetInputFilesImpl;
                disableCompressionSymbol = RegisterTag(Builder.DoNotCompressTag, () => Builder.DoNotCompressTag);
                Version = 3;
            }

            private IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                yield return new ObjectUrl(UrlType.File, Parameters.SourcePathFromDisk);
            }

            private ResultStatus Import(ICommandContext commandContext, TextureTool textureTool, TexImage texImage, TextureHelper.ImportParameters convertParameters)
            {
                var assetManager = new ContentManager();
                bool useSeparateDataContainer = Parameters.IsStreamable && (texImage.Dimension == TexImage.TextureDimension.Texture2D || texImage.Dimension == TexImage.TextureDimension.TextureCube);

                // Note: for streamable textures we want to store mip maps in a separate storage container and read them on request instead of whole asset deserialization (at once)

                if (useSeparateDataContainer)
                {
                    // Perform normal texture importing (but don't save it to file now)
                    var importResult = TextureHelper.ImportTextureImageRaw(textureTool, texImage, convertParameters, CancellationToken, commandContext.Logger);
                    if (importResult != ResultStatus.Successful)
                        return importResult;

                    // Make sure we don't compress mips data
                    var dataUrl = Url + "_Data";
                    commandContext.AddTag(new ObjectUrl(UrlType.ContentLink, dataUrl), disableCompressionSymbol);
                    
                    using (var outputImage = textureTool.ConvertToXenkoImage(texImage))
                    {
                        if (CancellationToken.IsCancellationRequested)
                            return ResultStatus.Cancelled;

                        // Create texture mips data containers (storage all array slices for every mip in separate chunks)
                        var desc = outputImage.Description;
                        List<byte[]> mipsData = new List<byte[]>(desc.MipLevels);
                        for (int mipIndex = 0; mipIndex < desc.MipLevels; mipIndex++)
                        {
                            int totalSize = 0;
                            for (int arrayIndex = 0; arrayIndex < desc.ArraySize; arrayIndex++)
                            {
                                var pixelBuffer = outputImage.GetPixelBuffer(arrayIndex, 0, mipIndex);
                                totalSize += pixelBuffer.BufferStride;
                            }

                            var buf = new byte[totalSize];
                            int startIndex = 0;
                            for (int arrayIndex = 0; arrayIndex < desc.ArraySize; arrayIndex++)
                            {
                                var pixelBuffer = outputImage.GetPixelBuffer(arrayIndex, 0, mipIndex);
                                int size = pixelBuffer.BufferStride;

                                Marshal.Copy(pixelBuffer.DataPointer, buf, startIndex, size);
                                startIndex += size;
                            }
                            mipsData.Add(buf);
                        }

                        // Pack mip maps to the storage container
                        ContentStorageHeader storageHeader;
                        ContentStorage.Create(dataUrl, mipsData, out storageHeader);
                        
                        if (CancellationToken.IsCancellationRequested)
                            return ResultStatus.Cancelled;

                        // Serialize texture to file
                        var outputTexture = new TextureSerializationData(outputImage, Parameters.IsStreamable, storageHeader);
                        assetManager.Save(convertParameters.OutputUrl, outputTexture.ToSerializableVersion());

                        commandContext.Logger.Verbose($"Compression successful [{dataUrl}] to ({outputImage.Description.Width}x{outputImage.Description.Height},{outputImage.Description.Format})");
                    }

                    return ResultStatus.Successful;
                }

                // Import texture and save to file
                return TextureHelper.ImportTextureImage(assetManager, textureTool, texImage, convertParameters, CancellationToken, commandContext.Logger);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var convertParameters = new TextureHelper.ImportParameters(Parameters) { OutputUrl = Url };

                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(Parameters.SourcePathFromDisk, convertParameters.IsSRgb))
                {
                    var importResult = Import(commandContext, texTool, texImage, convertParameters);

                    return Task.FromResult(importResult);
                }
            }

            protected override void ComputeAssemblyHash(BinarySerializationWriter writer)
            {
                writer.Write(DataSerializer.BinaryFormatVersion);
                writer.Write(TextureSerializationData.Version);

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
            IsCompressed = texture.IsCompressed;
            IsStreamable = texture.IsStreamable;
            Platform = platform;
            GraphicsPlatform = graphicsPlatform;
            GraphicsProfile = graphicsProfile;
            TextureQuality = textureQuality;
            ColorSpace = colorSpace;
        }

        public UFile SourcePathFromDisk;

        public TextureAsset Texture;

        public bool IsStreamable;

        public bool IsCompressed;

        public PlatformType Platform;

        public GraphicsPlatform GraphicsPlatform;

        public GraphicsProfile GraphicsProfile;

        public TextureQuality TextureQuality;

        public ColorSpace ColorSpace;
    }
}
