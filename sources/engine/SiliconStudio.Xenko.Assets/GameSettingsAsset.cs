// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetContentType(typeof(GameSettings))]
    [Display(10000, "Game settings")]
    [CategoryOrder(4050, "Splash screen")]
    [NonIdentifiableCollectionItems]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "1.6.1-alpha01")]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.1-alpha01", "1.9.3-alpha01", typeof(UpgradeAddAudioSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.3-alpha01", "1.11.0.0", typeof(UpgradeAddNavigationSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.11.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    [AssetUpgrader(XenkoConfig.PackageName, "2.0.0.0", "2.1.0.3", typeof(UpgradeAddStreamingSettings))]
    public class GameSettingsAsset : Asset
    {
        private const string CurrentVersion = "2.1.0.3";

        /// <summary>
        /// The default file extension used by the <see cref="GameSettingsAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgamesettings";

        public const string GameSettingsLocation = GameSettings.AssetUrl;

        public const string DefaultSceneLocation = "MainScene";

        /// <summary>
        /// Gets or sets the default scene
        /// </summary>
        /// <userdoc>The default scene loaded when the game starts</userdoc>
        [DataMember(1000)]
        public Scene DefaultScene { get; set; }

        [DataMember(1500)]
        public GraphicsCompositor GraphicsCompositor { get; set; }

        /// <userdoc>
        /// The image (eg company logo) displayed as the splash screen
        /// </userdoc>
        [Display("Texture", "Splash screen")]
        [DataMember(5000)]
        public Texture SplashScreenTexture { get; set; }

        /// <userdoc>
        /// The color the splash screen fades in on top of
        /// </userdoc>
        [Display("Color", "Splash screen")]
        [DataMember(5050)]
        public Color SplashScreenColor { get; set; } = Color.Black;

        [DataMember(2000)]
        [MemberCollection(ReadOnly = true, NotNullItems = true)]
        public List<Configuration> Defaults { get; } = new List<Configuration>();

        [DataMember(3000)]
        [Category]
        public List<ConfigurationOverride> Overrides { get; } = new List<ConfigurationOverride>();

        [DataMember(4000)]
        [Category]
        public List<string> PlatformFilters { get; } = new List<string>();

        /// <summary>
        /// Tries to get the requested <see cref="Configuration"/>, returns null if it doesn't exist
        /// </summary>
        /// <typeparam name="T">The <see cref="Configuration"/> to get</typeparam>
        /// <param name="profile">If not null, will filter the results by profile first</param>
        /// <returns></returns>
        public T TryGet<T>(string profile = null) where T : Configuration
        {
            if (profile != null)
            {
                foreach (var configurationOverride in Overrides)
                {
                    if (configurationOverride.SpecificFilter == -1) continue;
                    var filter = PlatformFilters[configurationOverride.SpecificFilter];
                    if (filter == profile)
                    {
                        var x = configurationOverride.Configuration;
                        if (x?.GetType() == typeof(T))
                            return (T)x;
                    }
                }
            }

            foreach (var x in Defaults)
            {
                if (x?.GetType() == typeof(T))
                    return (T)x;
            }

            return null;
        }

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

        private class UpgradeAddStreamingSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                dynamic settings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Streaming.StreamingSettings,SiliconStudio.Xenko.Engine" });
                asset.Defaults.Add(settings);
            }
        }
    }
}
