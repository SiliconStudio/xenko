using System;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
    public class GameSettingsAssetCompiler : AssetCompilerBase<GameSettingsAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, GameSettingsAsset asset, AssetCompilerResult result)
        {
            var compilationMode = CompilationMode.Debug;
            switch (context.BuildConfiguration)
            {
                case "Debug":
                    compilationMode = CompilationMode.Debug;
                    break;
                case "Release":
                    compilationMode = CompilationMode.Release;
                    break;
                case "AppStore":
                    compilationMode = CompilationMode.AppStore;
                    break;
                case "Testing":
                    compilationMode = CompilationMode.Testing;
                    break;
            }

            // TODO: We should ignore game settings stored in dependencies
            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new GameSettingsCompileCommand(urlInStorage, AssetItem.Package, context.Platform, compilationMode, asset),
            };
        }

        private class GameSettingsCompileCommand : AssetCommand<GameSettingsAsset>
        {
            private readonly Package package;
            private readonly PlatformType platform;
            private readonly CompilationMode compilationMode;

            public GameSettingsCompileCommand(string url, Package package, PlatformType platform, CompilationMode compilationMode, GameSettingsAsset asset)
                : base(url, asset)
            {
                this.package = package;
                this.platform = platform;
                this.compilationMode = compilationMode;
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                // Hash used parameters from package
                writer.Write(package.Id);
                writer.Write(package.UserSettings.GetValue(GameUserSettings.Effect.EffectCompilation));
                writer.Write(package.UserSettings.GetValue(GameUserSettings.Effect.RecordUsedEffects));
                writer.Write(compilationMode);

                // Hash platform
                writer.Write(platform);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var result = new GameSettings
                {
                    PackageId = package.Id,
                    PackageName = package.Meta.Name,
                    DefaultSceneUrl = AssetParameters.DefaultScene != null ? AttachedReferenceManager.GetUrl(AssetParameters.DefaultScene) : null,
                    DefaultBackBufferWidth = AssetParameters.BackBufferWidth,
                    DefaultBackBufferHeight = AssetParameters.BackBufferHeight,
                    DefaultGraphicsProfileUsed = AssetParameters.DefaultGraphicsProfile,
                    ColorSpace =  AssetParameters.ColorSpace,
                    EffectCompilation = package.UserSettings.GetValue(GameUserSettings.Effect.EffectCompilation),
                    RecordUsedEffects = package.UserSettings.GetValue(GameUserSettings.Effect.RecordUsedEffects),
                    CompilationMode = compilationMode
                };

                // TODO: Platform-specific settings have priority
                //if (platform != PlatformType.Shared)
                //{
                //    var platformProfile = package.Profiles.FirstOrDefault(o => o.Platform == platform);
                //    if (platformProfile != null && platformProfile.Properties.ContainsKey(DefaultGraphicsProfile))
                //    {
                //        var customProfile = platformProfile.Properties.Get(DefaultGraphicsProfile);
                //        result.DefaultGraphicsProfileUsed = customProfile;
                //    }
                //}

                var assetManager = new AssetManager();
                assetManager.Save(Url, result);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}