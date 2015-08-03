// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Settings;
using SiliconStudio.Paradox.Assets.Entities;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [AssetDescription(FileExtension, false)]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    [AssetCompiler(typeof(GameSettingsAssetCompiler))]
    [Display(80, "Game Settings", "A game settings asset")]
    public class GameSettingsAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="GameSettingsAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxgamesettings";

        public const string GameSettingsLocation = GameSettings.AssetUrl;
        public const string DefaultSceneLocation = "MainScene";

        public GameSettingsAsset()
        {
            BackBufferWidth = 1280;
            BackBufferHeight = 720;
            DefaultGraphicsProfile = GraphicsProfile.Level_10_0;
        }

        /// <summary>
        /// Gets or sets the default scene.
        /// </summary>
        /// <userdoc>The default scene that will be loaded at game startup.</userdoc>
        public Scene DefaultScene { get; set; }

        /// <summary>
        /// Gets or sets the width of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer width.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        public int BackBufferWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer height.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        public int BackBufferHeight { get; set; }

        /// <summary>
        /// Gets or sets the default graphics profile.
        /// </summary>
        /// <userdoc>The graphics feature level this game require.</userdoc>
        public GraphicsProfile DefaultGraphicsProfile { get; set; }

        /// <summary>
        /// Gets or sets the display orientation.
        /// </summary>
        /// <userdoc>The display orientations this game support.</userdoc>
        public DisplayOrientation DisplayOrientation { get; set; }

        /// <summary>
        /// Gets or sets the texture quality.
        /// </summary>
        /// <userdoc>The texture quality when encoding textures. Higher settings might result in much slower build depending on the target platform.</userdoc>
        public TextureQuality TextureQuality { get; set; }

        internal class UpgraderVersion122
        {
            public static SettingsValueKey<DisplayOrientation> DisplayOrientation = new SettingsValueKey<DisplayOrientation>("Paradox.DisplayOrientation", PackageProfile.SettingsGroup);

            public static SettingsValueKey<GraphicsPlatform> GraphicsPlatform = new SettingsValueKey<GraphicsPlatform>("Paradox.GraphicsPlatform", PackageProfile.SettingsGroup);

            public static SettingsValueKey<TextureQuality> TextureQuality = new SettingsValueKey<TextureQuality>("Paradox.TextureQuality", PackageProfile.SettingsGroup);

            public static readonly SettingsValueKey<AssetReference<SceneAsset>> DefaultScene = new SettingsValueKey<AssetReference<SceneAsset>>("GameSettingsAsset.DefaultScene", PackageProfile.SettingsGroup);

            public static readonly SettingsValueKey<int> BackBufferWidth = new SettingsValueKey<int>("GameSettingsAsset.BackBufferWidth", PackageProfile.SettingsGroup, 1280);

            public static readonly SettingsValueKey<int> BackBufferHeight = new SettingsValueKey<int>("GameSettingsAsset.BackBufferHeight", PackageProfile.SettingsGroup, 720);

            public static readonly SettingsValueKey<GraphicsProfile> DefaultGraphicsProfile = new SettingsValueKey<GraphicsProfile>("GameSettingsAsset.DefaultGraphicsProfile", PackageProfile.SettingsGroup, GraphicsProfile.Level_10_0);

            // Gets the default scene from a package properties
            public static AssetReference<SceneAsset> GetDefaultScene(Package package)
            {
                var packageSharedProfile = package.Profiles.FindSharedProfile();
                if (packageSharedProfile == null) return null;
                return packageSharedProfile.Properties.Get(DefaultScene);
            }

            public static int GetBackBufferWidth(Package package)
            {
                var packageSharedProfile = package.Profiles.FindSharedProfile();
                if (packageSharedProfile == null) return 0;
                return packageSharedProfile.Properties.Get(BackBufferWidth);
            }

            public static int GetBackBufferHeight(Package package)
            {
                var packageSharedProfile = package.Profiles.FindSharedProfile();
                if (packageSharedProfile == null) return 0;
                return packageSharedProfile.Properties.Get(BackBufferHeight);
            }

            public static GraphicsProfile GetGraphicsProfile(Package package)
            {
                var packageSharedProfile = package.Profiles.FindSharedProfile();
                if (packageSharedProfile == null) return 0;
                return packageSharedProfile.Properties.Get(DefaultGraphicsProfile);
            }

            public static bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
            {
                var packageSharedProfile = dependentPackage.Profiles.FindSharedProfile();

                // Only do something if there is a default scene defined
                if (packageSharedProfile != null && packageSharedProfile.Properties.ContainsKey(DefaultScene))
                {
                    var defaultScene = packageSharedProfile.Properties.Get(DefaultScene);

                    var defaultGraphicsProfile = packageSharedProfile.Properties.Get(DefaultGraphicsProfile);

                    // If available, use graphics profile from Windows platform
                    foreach (var profile in dependentPackage.Profiles)
                    {
                        if (profile.Platform == PlatformType.Windows && profile.Properties.ContainsKey(DefaultGraphicsProfile))
                        {
                            defaultGraphicsProfile = profile.Properties.Get(DefaultGraphicsProfile);
                        }
                    }

                    // Create asset
                    var gameSettingsAsset = new GameSettingsAsset
                    {
                        DefaultScene = AttachedReferenceManager.CreateSerializableVersion<Scene>(defaultScene.Id, defaultScene.Location),
                        BackBufferWidth = packageSharedProfile.Properties.Get(BackBufferWidth),
                        BackBufferHeight = packageSharedProfile.Properties.Get(BackBufferHeight),
                        DefaultGraphicsProfile = defaultGraphicsProfile,
                        DisplayOrientation = packageSharedProfile.Properties.Get(DisplayOrientation),
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
                        profile.Properties.Remove(DefaultScene);
                        profile.Properties.Remove(BackBufferWidth);
                        profile.Properties.Remove(BackBufferHeight);
                        profile.Properties.Remove(DefaultGraphicsProfile);
                        profile.Properties.Remove(DisplayOrientation);
                    }
                }

                return true;
            }
        }
    }
}
