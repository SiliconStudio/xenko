// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    internal class SkyboxAssetCompiler : AssetCompilerBase
    {
        public SkyboxAssetCompiler()
        {
            CompileTimeDependencyTypes.Add(typeof(TextureAsset), BuildDependencyType.CompileContent);
        }

        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (SkyboxAsset)assetItem.Asset;
            result.BuildSteps = new ListBuildStep();
            result.ShouldWaitForPreviousBuilds = true;

            var colorSpace = context.GetColorSpace();

            // build the textures for windows (needed for skybox compilation)
            foreach (var dependency in asset.Model.GetDependencies())
            {
                var dependencyItem = assetItem.Package.Assets.Find(dependency.Id);
                if (dependencyItem?.Asset is TextureAsset)
                {
                    var textureAsset = (TextureAsset)dependencyItem.Asset;

                    // Get absolute path of asset source on disk
                    var assetSource = GetAbsolutePath(dependencyItem, textureAsset.Source);

                    // Create a synthetic url
                    var textureUrl = SkyboxGenerator.BuildTextureForSkyboxGenerationLocation(dependencyItem.Location);

                    var gameSettingsAsset = context.GetGameSettingsAsset();
                    var renderingSettings = gameSettingsAsset.Get<RenderingSettings>(context.Platform);

                    // Select the best graphics profile
                    var graphicsProfile = renderingSettings.DefaultGraphicsProfile >= GraphicsProfile.Level_10_0 ? renderingSettings.DefaultGraphicsProfile : GraphicsProfile.Level_10_0;

                    var textureAssetItem = new AssetItem(textureUrl, textureAsset);

                    // Create and add the texture command.
                    var textureParameters = new TextureConvertParameters(assetSource, textureAsset, PlatformType.Windows, GraphicsPlatform.Direct3D11, graphicsProfile, gameSettingsAsset.Get<TextureSettings>().TextureQuality, colorSpace);
                    result.BuildSteps.Add(new AssetBuildStep(textureAssetItem) { new TextureAssetCompiler.TextureConvertCommand(textureUrl, textureParameters, assetItem.Package) });
                }
            }

            // add the skybox command itself.
            result.BuildSteps.Add(new AssetBuildStep(assetItem) {  new SkyboxCompileCommand(targetUrlInStorage, asset, assetItem.Package) });
        }

        private class SkyboxCompileCommand : AssetCommand<SkyboxAsset>
        {
            public SkyboxCompileCommand(string url, SkyboxAsset parameters, Package package)
                : base(url, parameters, package)
            {
            }

            /// <inheritdoc/>
            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                writer.Write(1); // Change this number to recompute the hash when prefiltering algorithm are changed
            }

            /// <inheritdoc/>
            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                foreach (var dependency in Parameters.Model.GetDependencies())
                {
                    // Use UrlType.Content instead of UrlType.Link, as we are actualy using the content linked of assets in order to compute the skybox
                    yield return new ObjectUrl(UrlType.Content, SkyboxGenerator.BuildTextureForSkyboxGenerationLocation(dependency.Location));
                }
            }

            /// <inheritdoc/>
            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // TODO Convert SkyboxAsset to Skybox and save to Skybox object
                // TODO Add system to prefilter

                using (var context = new SkyboxGeneratorContext(Parameters))
                {
                    var result = SkyboxGenerator.Compile(Parameters, context);

                    if (result.HasErrors)
                    {
                        result.CopyTo(commandContext.Logger);
                        return Task.FromResult(ResultStatus.Failed);
                    }

                    context.Content.Save(Url, result.Skybox);
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
