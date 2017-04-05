// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets
{
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtensions, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetContentType(typeof(GameSettings))]
    [Display(10000, "Game Settings")]
    [NonIdentifiableCollectionItems]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.6.1-alpha01")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.1-alpha01", "1.9.3-alpha01", typeof(UpgradeAddAudioSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.3-alpha01", "1.11.0.0", typeof(UpgradeAddNavigationSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.11.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public class GameSettingsAsset : Asset
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="GameSettingsAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgamesettings";

        public const string FileExtensions = FileExtension + ";.pdxgamesettings";

        public const string GameSettingsLocation = GameSettings.AssetUrl;

        public const string DefaultSceneLocation = "MainScene";

        /// <summary>
        /// Gets or sets the default scene.
        /// </summary>
        /// <userdoc>The default scene that will be loaded at game startup.</userdoc>
        [DataMember(1000)]
        public Scene DefaultScene { get; set; }

        [DataMember(1500)]
        public GraphicsCompositor GraphicsCompositor { get; set; }

        [DataMember(2000)]
        [MemberCollection(ReadOnly = true, NotNullItems = true)]
        public List<Configuration> Defaults { get; } = new List<Configuration>();

        [DataMember(3000)]
        [Category]
        public List<ConfigurationOverride> Overrides { get; } = new List<ConfigurationOverride>();

        [DataMember(4000)]
        [Category]
        public List<string> PlatformFilters { get; } = new List<string>();

        public T GetOrCreate<T>(string profile = null) where T : Configuration, new()
        {
            Configuration first = null;
            if (profile != null)
            {
                foreach (var configurationOverride in Overrides)
                {
                    if (configurationOverride.SpecificFilter == -1) continue;
                    var filter = PlatformFilters[configurationOverride.SpecificFilter];
                    if (filter == profile)
                    {
                        var x = configurationOverride.Configuration;
                        if (x != null && x.GetType() == typeof(T))
                        {
                            first = x;
                            break;
                        }
                    }
                }
            }
            if (first == null)
            {
                foreach (var x in Defaults)
                {
                    if (x != null && x.GetType() == typeof(T))
                    {
                        first = x;
                        break;
                    }
                }
            }
            var settings = (T)first;
            if (settings != null) return settings;
            settings = ObjectFactoryRegistry.NewInstance<T>();
            Defaults.Add(settings);
            return settings;
        }

        public T GetOrCreate<T>(PlatformType platform) where T : Configuration, new()
        {
            ConfigPlatforms configPlatform;
            switch (platform)
            {
                case PlatformType.Windows:
                    configPlatform = ConfigPlatforms.Windows;
                    break;
                case PlatformType.Android:
                    configPlatform = ConfigPlatforms.Android;
                    break;
                case PlatformType.iOS:
                    configPlatform = ConfigPlatforms.iOS;
                    break;
                case PlatformType.UWP:
                    configPlatform = ConfigPlatforms.UWP;
                    break;
                case PlatformType.Linux:
                    configPlatform = ConfigPlatforms.Linux;
                    break;
                case PlatformType.macOS:
                    configPlatform = ConfigPlatforms.macOS;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
            var platVersion = Overrides.FirstOrDefault(x => x != null && x.Platforms.HasFlag(configPlatform) && x.Configuration is T);
            if (platVersion != null)
            {
                return (T)platVersion.Configuration;
            }

            return GetOrCreate<T>();
        }

        private class UpgradeAddAudioSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Audio.AudioEngineSettings,SiliconStudio.Xenko.Audio" });
                asset.Defaults.Add(setting);
            }
        }

        private class UpgradeAddNavigationSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                dynamic settings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Navigation.NavigationSettings,SiliconStudio.Xenko.Navigation" });

                // Default build settings
                dynamic buildSettings = new DynamicYamlMapping(new YamlMappingNode());
                buildSettings.CellHeight = 0.2f;
                buildSettings.CellSize = 0.3f;
                buildSettings.TileSize = 32;
                buildSettings.MinRegionArea = 2;
                buildSettings.RegionMergeArea = 20;
                buildSettings.MaxEdgeLen = 12.0f;
                buildSettings.MaxEdgeError = 1.3f;
                buildSettings.DetailSamplingDistance = 6.0f;
                buildSettings.MaxDetailSamplingError = 1.0f;
                settings.BuildSettings = buildSettings;

                var groups = new DynamicYamlArray(new YamlSequenceNode());
                
                // Agent settings array
                settings.Groups = groups;

                asset.Defaults.Add(settings);
            }
        }
    }
}
