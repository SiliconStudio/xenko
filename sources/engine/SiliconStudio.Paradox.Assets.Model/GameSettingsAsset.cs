// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Model
{
    /// <summary>
    /// Settings for a game with the default scene, resolution, graphics profile...
    /// </summary>
    [DataContract("GameSettingsAsset")]
    [ContentSerializer(typeof(DataContentSerializer<GameSettingsAsset>))]
    public class GameSettingsAsset
    {

        public static readonly PropertyKey<AssetReference<SceneAsset>> DefaultScene = new PropertyKey<AssetReference<SceneAsset>>("DefaultScene", typeof(GameSettingsAsset));

        public static readonly PropertyKey<int> BackBufferWidth = new PropertyKey<int>("BackBufferWidth", typeof(GameSettingsAsset));

        public static readonly PropertyKey<int> BackBufferHeight = new PropertyKey<int>("BackBufferHeight", typeof(GameSettingsAsset));

        public static readonly PropertyKey<GraphicsProfile> DefaultGraphicsProfile = new PropertyKey<GraphicsProfile>("DefaultGraphicsProfile", typeof(GameSettingsAsset));


        // Gets the default scene from a package properties
        public static AssetReference<SceneAsset> GetDefaultScene(Package package)
        {
            var packageSharedProfile = package.Profiles.FindSharedProfile();
            if (packageSharedProfile == null) return null;
            return packageSharedProfile.Properties.Get(DefaultScene);
        }

        // Sets the default scene within a package properties
        public static void SetDefaultScene(Package package, AssetReference<SceneAsset> defaultScene)
        {
            package.Profiles.FindSharedProfile().Properties.Set(DefaultScene, defaultScene);
            MarkPackageDirty(package);
        }

        public static int GetBackBufferWidth(Package package)
        {
            var packageSharedProfile = package.Profiles.FindSharedProfile();
            if (packageSharedProfile == null) return 0;
            return packageSharedProfile.Properties.Get(BackBufferWidth);
        }

        public static void SetBackBufferWidth(Package package, int value)
        {
            package.Profiles.FindSharedProfile().Properties.Set(BackBufferWidth, value);
            MarkPackageDirty(package);
        }

        public static int GetBackBufferHeight(Package package)
        {
            var packageSharedProfile = package.Profiles.FindSharedProfile();
            if (packageSharedProfile == null) return 0;
            return packageSharedProfile.Properties.Get(BackBufferHeight);
        }

        public static void SetBackBufferHeight(Package package, int value)
        {
            package.Profiles.FindSharedProfile().Properties.Set(BackBufferHeight, value);
            MarkPackageDirty(package);
        }

        public static void SetGraphicsProfile(Package package, GraphicsProfile value)
        {
            package.Profiles.FindSharedProfile().Properties.Set(DefaultGraphicsProfile, value);
            MarkPackageDirty(package);
        }

        public static GraphicsProfile GetGraphicsProfile(Package package)
        {
            var packageSharedProfile = package.Profiles.FindSharedProfile();
            if (packageSharedProfile == null) return 0;
            return packageSharedProfile.Properties.Get(DefaultGraphicsProfile);
        }

        public static void MarkPackageDirty(Package package)
        {
            package.IsDirty = true;
        }

        // Build a full GameSettings from a package
        public static GameSettings CreateFromPackage(Package package)
        {
            var result = new GameSettings();

            // Default scene
            var sharedProfile = package.Profiles.FindSharedProfile();
            if (sharedProfile != null)
            {
                var sceneAsset = sharedProfile.Properties.Get(DefaultScene);
                if (sceneAsset != null) result.DefaultSceneUrl = sceneAsset.Location;
                result.DefaultBackBufferWidth = sharedProfile.Properties.Get(BackBufferWidth);
                result.DefaultBackBufferHeight = sharedProfile.Properties.Get(BackBufferHeight);
                result.DefaultGraphicsProfileUsed = sharedProfile.Properties.Get(DefaultGraphicsProfile);
            }

            return result;
        }

        public static void CreateAndSetDefaultScene(Package package, String location = "MainScene")
        {
            var defaultSceneAsset = SceneAsset.Create();

            var sceneAssetItem = new AssetItem(location, defaultSceneAsset);
            package.Assets.Add(sceneAssetItem);
            sceneAssetItem.IsDirty = true;
            var sceneAsset = new AssetReference<SceneAsset>(sceneAssetItem.Id, sceneAssetItem.Location);

            // Sets the scene created as default in the shared profile
            SetDefaultScene(package, sceneAsset);
        }
        
    }
}
