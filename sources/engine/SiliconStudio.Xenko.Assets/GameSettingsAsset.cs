// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets
{ 
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtensions, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetCompiler(typeof(GameSettingsAssetCompiler))]
    [AssetContentType(typeof(GameSettings))]
    [Display(10000, "Game Settings")]
    [NonIdentifiableCollectionItems]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, "0", "1.6.0-beta", typeof(UpgraderPlatformsConfiguration))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta", "1.6.1-alpha01", typeof(UpgradeNewGameSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.1-alpha01", "1.9.3-alpha01", typeof(UpgradeAddAudioSettings))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.9.3-alpha01", "1.11.0", typeof(UpgradeAddNavigationSettings))]
    public class GameSettingsAsset : Asset
    {
        private const string CurrentVersion = "1.11.0";

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
            settings = new T();
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

        internal class UpgraderPlatformsConfiguration : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                int backBufferWidth = asset.BackBufferWidth ?? 1280;
                asset.RemoveChild("BackBufferWidth");
                int backBufferHeight = asset.BackBufferHeight ?? 720;
                asset.RemoveChild("BackBufferHeight");
                GraphicsProfile profile = asset.DefaultGraphicsProfile ?? GraphicsProfile.Level_9_1;
                asset.RemoveChild("DefaultGraphicsProfile");
                ColorSpace colorSpace = asset.ColorSpace ?? ColorSpace.Linear;
                asset.RemoveChild("ColorSpace");
                DisplayOrientation displayOrientation = asset.DisplayOrientation ?? DisplayOrientation.Default;
                asset.RemoveChild("DisplayOrientation");
                TextureQuality textureQuality = asset.TextureQuality ?? TextureQuality.Fast;
                asset.RemoveChild("TextureQuality");
                var renderingMode = RenderingMode.HDR;
                if (asset.RenderingMode != null)
                {
                    if (asset.RenderingMode == "LDR")
                    {
                        renderingMode = RenderingMode.LDR;
                    }
                }
                asset.RemoveChild("RenderingMode");

                var configurations = new DynamicYamlArray(new YamlSequenceNode());
                asset.Defaults = configurations;

                dynamic renderingSettings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Graphics.RenderingSettings,SiliconStudio.Xenko.Graphics" });
                renderingSettings.DefaultBackBufferWidth = backBufferWidth;
                renderingSettings.DefaultBackBufferHeight = backBufferHeight;
                renderingSettings.DefaultGraphicsProfile = profile;
                renderingSettings.ColorSpace = colorSpace;
                renderingSettings.DisplayOrientation = displayOrientation;
                asset.Defaults.Add(renderingSettings);

                dynamic editorSettings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Assets.EditorSettings,SiliconStudio.Xenko.Assets" });
                editorSettings.RenderingMode = renderingMode;
                asset.Defaults.Add(editorSettings);

                dynamic textSettings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Assets.Textures.TextureSettings,SiliconStudio.Xenko.Assets" });
                textSettings.TextureQuality = textureQuality;
                asset.Defaults.Add(textSettings);

                dynamic physicsSettings = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Physics.PhysicsSettings,SiliconStudio.Xenko.Physics" });
                asset.Defaults.Add(physicsSettings);

                var defaultFilters = new DynamicYamlArray(new YamlSequenceNode());
                asset.PlatformFilters = defaultFilters;
                asset.PlatformFilters.Add("PowerVR SGX 54[0-9]");
                asset.PlatformFilters.Add("Adreno \\(TM\\) 2[0-9][0-9]");
                asset.PlatformFilters.Add("Adreno (TM) 320");
                asset.PlatformFilters.Add("Adreno (TM) 330");
                asset.PlatformFilters.Add("Adreno \\(TM\\) 4[0-9][0-9]");
                asset.PlatformFilters.Add("NVIDIA Tegra");
                asset.PlatformFilters.Add("Intel(R) HD Graphics");
                asset.PlatformFilters.Add("^Mali\\-4");
                asset.PlatformFilters.Add("^Mali\\-T6");
                asset.PlatformFilters.Add("^Mali\\-T7");
            }
        }

        internal class UpgradeNewGameSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var addRendering = true;
                var addEditor = true;
                var addPhysics = true;
                var addTexture = true;
                foreach (DynamicYamlMapping mapping in asset.Defaults)
                {
                    if (mapping.Node.Tag == "!SiliconStudio.Xenko.Graphics.RenderingSettings,SiliconStudio.Xenko.Graphics") addRendering = false;
                    if (mapping.Node.Tag == "!SiliconStudio.Xenko.Assets.EditorSettings,SiliconStudio.Xenko.Assets") addEditor = false;
                    if (mapping.Node.Tag == "!SiliconStudio.Xenko.Assets.Textures.TextureSettings,SiliconStudio.Xenko.Assets") addTexture = false;
                    if (mapping.Node.Tag == "!SiliconStudio.Xenko.Physics.PhysicsSettings,SiliconStudio.Xenko.Physics") addPhysics = false;
                }

                if (addRendering)
                {
                    dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Graphics.RenderingSettings,SiliconStudio.Xenko.Graphics" });
                    asset.Defaults.Add(setting);
                }

                if (addEditor)
                {
                    dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Assets.EditorSettings,SiliconStudio.Xenko.Assets" });
                    asset.Defaults.Add(setting);
                }

                if (addPhysics)
                {
                    dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Physics.PhysicsSettings,SiliconStudio.Xenko.Physics" });
                    asset.Defaults.Add(setting);
                }

                if (addTexture)
                {
                    dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Assets.Textures.TextureSettings,SiliconStudio.Xenko.Assets" });
                    asset.Defaults.Add(setting);
                }
            }
        }

        internal class UpgradeAddAudioSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Audio.AudioEngineSettings,SiliconStudio.Xenko.Audio" });
                asset.Defaults.Add(setting);
            }
        }

        internal class UpgradeAddNavigationSettings : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                dynamic setting = new DynamicYamlMapping(new YamlMappingNode { Tag = "!SiliconStudio.Xenko.Navigation.NavigationSettings,SiliconStudio.Xenko.Navigation" });
                asset.Defaults.Add(setting);
            }
        }
    }
}
