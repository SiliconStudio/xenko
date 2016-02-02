// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Settings;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{ 
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtensions, false, AlwaysMarkAsRoot = true)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetCompiler(typeof(GameSettingsAssetCompiler))]
    [Display(80, "Game Settings")]
    public class GameSettingsAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="GameSettingsAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgamesettings";

        public const string FileExtensions = FileExtension + ";.pdxgamesettings";

        public const string GameSettingsLocation = GameSettings.AssetUrl;
        public const string DefaultSceneLocation = "MainScene";

        public GameSettingsAsset()
        {
            BackBufferWidth = 1280;
            BackBufferHeight = 720;
            DefaultGraphicsProfile = GraphicsProfile.Level_10_0;
            RenderingMode = RenderingMode.HDR;
            ColorSpace = ColorSpace.Linear;
        }

        /// <summary>
        /// Gets or sets the default scene.
        /// </summary>
        /// <userdoc>The default scene that will be loaded at game startup.</userdoc>
        [DataMember(10)]
        public Scene DefaultScene { get; set; }

        /// <summary>
        /// Gets or sets the width of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer width.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        [DataMember(20)]
        [Display(null, "Graphics")]
        public int BackBufferWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer height.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        [DataMember(30)]
        [Display(null, "Graphics")]
        public int BackBufferHeight { get; set; }

        /// <summary>
        /// Gets or sets the default graphics profile.
        /// </summary>
        /// <userdoc>The graphics feature level this game require.</userdoc>
        [DataMember(40)]
        [Display(null, "Graphics")]
        public GraphicsProfile DefaultGraphicsProfile { get; set; }

        /// <summary>
        /// Gets or sets the display orientation.
        /// </summary>
        /// <userdoc>The display orientations this game support.</userdoc>
        [DataMember(50)]
        [Display(null, "Graphics")]
        public DisplayOrientation DisplayOrientation { get; set; }

        /// <summary>
        /// Gets or sets the texture quality.
        /// </summary>
        /// <userdoc>The texture quality when encoding textures. Higher settings might result in much slower build depending on the target platform.</userdoc>
        [DataMember(60)]
        [Display(null, "Graphics")]
        public TextureQuality TextureQuality { get; set; }

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        /// <value>The rendering mode.</value>
        /// <userdoc>The default rendering mode (HDR or LDR) used to render the preview and thumbnail. This value doesn't affect the runtime but only the editor.</userdoc>
        [DataMember(70)]
        [DefaultValue(Assets.RenderingMode.HDR)]
        [Display("Editor Rendering Mode", "Graphics")]
        public RenderingMode RenderingMode { get; set; }

        /// <summary>
        /// Gets or sets the colorspace.
        /// </summary>
        /// <value>The colorspace.</value>
        /// <userdoc>The colorspace (Gamma or Linear) used for rendering. This value affects both the runtime and editor.</userdoc>
        [DataMember(80)]
        [DefaultValue(ColorSpace.Linear)]
        [Display(null, "Graphics")]
        public ColorSpace ColorSpace { get; set; }

        /// <summary>
        /// Gets or sets the game settings per profiles.
        /// </summary>
        [DataMember(90)]
        [Display(Browsable = false)]
        [DefaultValue(null)]
        public Dictionary<string, IGameSettingsProfile> Profiles { get; set; }

        [DataMember(100)]
        [NotNullItems]
        public List<Configuration> Defaults { get; } = new List<Configuration>();

        [DataMember(110)]
        public List<ConfigurationOverride> Overrides { get; } = new List<ConfigurationOverride>();

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
                    var gameSettingsAsset = new GameSettingsAsset
                    {
                        DefaultScene = AttachedReferenceManager.CreateSerializableVersion<Scene>(defaultScene.Id, defaultScene.Location),
                        BackBufferWidth = Get(packageSharedProfile.Properties, BackBufferWidth),
                        BackBufferHeight = Get(packageSharedProfile.Properties, BackBufferHeight),
                        DefaultGraphicsProfile = defaultGraphicsProfile,
                        DisplayOrientation = Get(packageSharedProfile.Properties, DisplayOrientation),
                    };

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
    }
}
