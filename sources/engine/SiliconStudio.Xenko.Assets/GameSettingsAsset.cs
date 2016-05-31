// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Settings;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets
{ 
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtensions, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetCompiler(typeof(GameSettingsAssetCompiler))]
    [Display(80, "Game Settings")]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetUpgrader(XenkoConfig.PackageName, "0", "1.6.0-beta", typeof(UpgraderPlatformsConfiguration))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.6.0-beta", "1.6.1-alpha01", typeof(UpgradeNewGameSettings))]
    public class GameSettingsAsset : Asset
    {
        private const string CurrentVersion = "1.6.1-alpha01";

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

        [DataMember(2000)]
        [NotNullItems]
        [MemberCollection(ReadOnly = true)]
        public List<Configuration> Defaults { get; } = new List<Configuration>();

        [DataMember(3000)]
        [Category]
        public List<ConfigurationOverride> Overrides { get; } = new List<ConfigurationOverride>();

        [DataMember(4000)]
        [Category]
        public List<string> PlatformFilters { get; } = new List<string>(); 

        public T Get<T>(string profile = null) where T : Configuration, new()
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

        public T Get<T>(PlatformType platform) where T : Configuration, new()
        {
            ConfigPlatforms configPlatform;
            switch (platform)
            {
                case PlatformType.Windows:
                    configPlatform = ConfigPlatforms.Windows;
                    break;
                case PlatformType.WindowsPhone:
                    configPlatform = ConfigPlatforms.WindowsPhone;
                    break;
                case PlatformType.WindowsStore:
                    configPlatform = ConfigPlatforms.WindowsStore;
                    break;
                case PlatformType.Android:
                    configPlatform = ConfigPlatforms.Android;
                    break;
                case PlatformType.iOS:
                    configPlatform = ConfigPlatforms.iOS;
                    break;
                case PlatformType.Windows10:
                    configPlatform = ConfigPlatforms.Windows10;
                    break;
                case PlatformType.Linux:
                    configPlatform = ConfigPlatforms.Linux;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
            var platVersion = Overrides.FirstOrDefault(x => x != null && x.Platforms.HasFlag(configPlatform) && x.Configuration is T);
            if (platVersion != null)
            {
                return (T)platVersion.Configuration;
            }

            return Get<T>();
        }

        internal class UpgraderVersion130
        {
            public static SettingsKey<DisplayOrientation> DisplayOrientation = new SettingsKey<DisplayOrientation>("Xenko.DisplayOrientation", PackageProfile.SettingsContainer);

            public static SettingsKey<GraphicsPlatform> GraphicsPlatform = new SettingsKey<GraphicsPlatform>("Xenko.GraphicsPlatform", PackageProfile.SettingsContainer);

            public static SettingsKey<TextureQuality> TextureQuality = new SettingsKey<TextureQuality>("Xenko.TextureQuality", PackageProfile.SettingsContainer);

            public static readonly SettingsKey<AssetReference<SceneAsset>> DefaultScene = new SettingsKey<AssetReference<SceneAsset>>("GameSettingsAsset.DefaultScene", PackageProfile.SettingsContainer);

            public static readonly SettingsKey<int> BackBufferWidth = new SettingsKey<int>("GameSettingsAsset.BackBufferWidth", PackageProfile.SettingsContainer, 1280);

            public static readonly SettingsKey<int> BackBufferHeight = new SettingsKey<int>("GameSettingsAsset.BackBufferHeight", PackageProfile.SettingsContainer, 720);

            public static readonly SettingsKey<GraphicsProfile> DefaultGraphicsProfile = new SettingsKey<GraphicsProfile>("GameSettingsAsset.DefaultGraphicsProfile", PackageProfile.SettingsContainer, GraphicsProfile.Level_10_0);

            public static T Get<T>(SettingsProfile profile, SettingsKey<T> key)
            {
                return key.GetValue(profile, true);
            }

            public static bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
            {
                var packageSharedProfile = dependentPackage.Profiles.FindSharedProfile();

                // Only do something if there is a default scene defined
                if (packageSharedProfile != null && packageSharedProfile.Properties.ContainsKey(DefaultScene))
                {
                    var defaultScene = Get(packageSharedProfile.Properties, DefaultScene);

                    var defaultGraphicsProfile = Get(packageSharedProfile.Properties, DefaultGraphicsProfile);

                    // If available, use graphics profile from Windows platform
                    foreach (var profile in dependentPackage.Profiles)
                    {
                        if (profile.Platform == PlatformType.Windows && profile.Properties.ContainsKey(DefaultGraphicsProfile))
                        {
                            defaultGraphicsProfile = Get(profile.Properties, DefaultGraphicsProfile);
                        }
                    }

                    // Create asset
                    var gameSettingsAsset = GameSettingsFactory.Create();
                    gameSettingsAsset.DefaultScene = AttachedReferenceManager.CreateProxyObject<Scene>(defaultScene.Id, defaultScene.Location);

                    var renderingSettings = gameSettingsAsset.Get<RenderingSettings>();
                    renderingSettings.DisplayOrientation = (RequiredDisplayOrientation) Get(packageSharedProfile.Properties, DisplayOrientation);
                    renderingSettings.ColorSpace = ColorSpace.Linear;
                    renderingSettings.DefaultBackBufferWidth = Get(packageSharedProfile.Properties, BackBufferWidth);
                    renderingSettings.DefaultBackBufferHeight = Get(packageSharedProfile.Properties, BackBufferHeight);
                    renderingSettings.DefaultGraphicsProfile = defaultGraphicsProfile;

                    // Add asset
                    using (var memoryStream = new MemoryStream())
                    {
                        AssetSerializer.Save(memoryStream, gameSettingsAsset, log);
                        assetFiles.Add(new PackageLoadingAssetFile(dependentPackage, GameSettingsLocation + FileExtension, null) { AssetContent = memoryStream.ToArray() });
                    }

                    // Clean properties
                    foreach (var profile in dependentPackage.Profiles)
                    {
                        profile.Properties.Remove(DefaultScene.Name);
                        profile.Properties.Remove(BackBufferWidth.Name);
                        profile.Properties.Remove(BackBufferHeight.Name);
                        profile.Properties.Remove(DefaultGraphicsProfile.Name);
                        profile.Properties.Remove(DisplayOrientation.Name);
                    }
                }

                return true;
            }
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
    }
}
