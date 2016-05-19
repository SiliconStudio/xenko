using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
    public class GameSettingsAssetCompiler : AssetCompilerBase<GameSettingsAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, GameSettingsAsset asset, AssetCompilerResult result)
        {
            // TODO: We should ignore game settings stored in dependencies
            result.BuildSteps = new AssetBuildStep(AssetItem)
            {
                new GameSettingsCompileCommand(urlInStorage, AssetItem.Package, context.Platform, context.GetCompilationMode(), asset),
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
                    EffectCompilation = package.UserSettings.GetValue(GameUserSettings.Effect.EffectCompilation),
                    RecordUsedEffects = package.UserSettings.GetValue(GameUserSettings.Effect.RecordUsedEffects),
                    Configurations = new PlatformConfigurations(),
                    CompilationMode = compilationMode
                };

                //start from the default platform and go down overriding

                foreach (var configuration in AssetParameters.Defaults.Where(x => !x.OfflineOnly))
                {
                    result.Configurations.Configurations.Add(new ConfigurationOverride
                    {
                        Platforms = ConfigPlatforms.None,
                        SpecificFilter = -1,
                        Configuration = configuration
                    });
                }

                foreach (var configurationOverride in AssetParameters.Overrides.Where(x => x.Configuration != null && !x.Configuration.OfflineOnly))
                {
                    result.Configurations.Configurations.Add(configurationOverride);
                }

                result.Configurations.PlatformFilters = AssetParameters.PlatformFilters;

                //make sure we modify platform specific files to set the wanted orientation
                SetPlatformOrientation(package, platform, AssetParameters.Get<RenderingSettings>().DisplayOrientation);

                var assetManager = new ContentManager();
                assetManager.Save(Url, result);

                return Task.FromResult(ResultStatus.Successful);
            }  
        }

        public static void SetPlatformOrientation(Package package, PlatformType platform, RequiredDisplayOrientation orientation)
        {
            foreach (var profile in package.Profiles)
            {
                if (profile.Platform != platform) continue;

                switch (profile.Platform)
                {
                    case PlatformType.Android:
                        {
                            var exeProjectLocation = profile.ProjectReferences.FirstOrDefault(x => x.Type == ProjectType.Executable);
                            if (exeProjectLocation == null) continue;

                            var path = exeProjectLocation.Location;
                            var activityFileName = package.Meta.Name + "Activity.cs";
                            var activityFile = UPath.Combine(path.GetFullDirectory(), new UFile(activityFileName));
                            if (!File.Exists(activityFile)) continue;

                            var activitySource = File.ReadAllText(activityFile);

                            string orientationString;
                            switch (orientation)
                            {
                                case RequiredDisplayOrientation.Default:
                                    orientationString = "Android.Content.PM.ScreenOrientation.Landscape";
                                    break;
                                case RequiredDisplayOrientation.LandscapeLeft:
                                    orientationString = "Android.Content.PM.ScreenOrientation.Landscape";
                                    break;
                                case RequiredDisplayOrientation.LandscapeRight:
                                    orientationString = "Android.Content.PM.ScreenOrientation.ReverseLandscape";
                                    break;
                                case RequiredDisplayOrientation.Portrait:
                                    orientationString = "Android.Content.PM.ScreenOrientation.Portrait";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            activitySource = Regex.Replace(activitySource, @"(\[Activity(?:.*[\n,\r]*)+?[\n,\r,\s]*ScreenOrientation\s*=\s*)([\w,\d,\.]+)(\s*,)", $"$1{orientationString}$3");

                            File.WriteAllText(activityFile, activitySource);
                        }
                        break;
                    case PlatformType.iOS:
                        {
                            var exeProjectLocation = profile.ProjectReferences.FirstOrDefault(x => x.Type == ProjectType.Executable);
                            if (exeProjectLocation == null) continue;

                            var path = exeProjectLocation.Location;
                            var plistFile = UPath.Combine(path.GetFullDirectory(), new UFile("Info.plist"));
                            if (!File.Exists(plistFile)) continue;

                            var xmlDoc = XDocument.Load(plistFile);
                            var orientationKey = xmlDoc.Descendants("key").FirstOrDefault(x => x.Value == "UISupportedInterfaceOrientations");
                            var orientationElement = ((XElement)orientationKey?.NextNode)?.Descendants("string").FirstOrDefault();
                            if (orientationElement != null)
                            {
                                switch (orientation)
                                {
                                    case RequiredDisplayOrientation.Default:
                                        orientationElement.Value = "UIInterfaceOrientationLandscapeRight";
                                        break;
                                    case RequiredDisplayOrientation.LandscapeLeft:
                                        orientationElement.Value = "UIInterfaceOrientationLandscapeLeft";
                                        break;
                                    case RequiredDisplayOrientation.LandscapeRight:
                                        orientationElement.Value = "UIInterfaceOrientationLandscapeRight";
                                        break;
                                    case RequiredDisplayOrientation.Portrait:
                                        orientationElement.Value = "UIInterfaceOrientationPortrait";
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            xmlDoc.Save(plistFile);
                        }
                        break;
                }
            }
        }
    }
}
