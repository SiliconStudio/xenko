// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
    public static class AssetCompilerContextExtensions
    {
        private static readonly PropertyKey<GameSettingsAsset> GameSettingsAssetKey = new PropertyKey<GameSettingsAsset>("GameSettingsAsset", typeof(AssetCompilerContextExtensions));

        public static GameSettingsAsset GetGameSettingsAsset(this AssetCompilerContext context)
        {
            return context.Properties.Get(GameSettingsAssetKey);
        }

        public static ColorSpace GetColorSpace(this AssetCompilerContext context)
        {
            var settings = context.GetGameSettingsAsset();
            return settings.ColorSpace;
        }

        public static void SetGameSettingsAsset(this AssetCompilerContext context, GameSettingsAsset gameSettingsAsset)
        {
            context.Properties.Set(GameSettingsAssetKey, gameSettingsAsset);
        }

        public static IGameSettingsProfile GetGameSettingsForCurrentProfile(this AssetCompilerContext context)
        {
            var gameSettings = context.GetGameSettingsAsset();
            IGameSettingsProfile gameSettingsProfile = null;
            if (gameSettings != null && gameSettings.Profiles != null)
            {
                gameSettings.Profiles.TryGetValue(context.Profile, out gameSettingsProfile);
            }
            // TODO: Return default game settings profile based on the platform
            return gameSettingsProfile;
        }

        public static GraphicsPlatform GetGraphicsPlatform(this AssetCompilerContext context)
        {
            var  gameSettingsProfile = GetGameSettingsForCurrentProfile(context);
            var graphicsPlatform =  gameSettingsProfile?.GraphicsPlatform ?? context.Platform.GetDefaultGraphicsPlatform();
            return graphicsPlatform;
        }

        public static Xenko.Graphics.GraphicsPlatform GetDefaultGraphicsPlatform(this PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                case PlatformType.WindowsPhone:
                case PlatformType.WindowsStore:
                case PlatformType.Windows10:
                    return Xenko.Graphics.GraphicsPlatform.Direct3D11;
                case PlatformType.Android:
                case PlatformType.iOS:
                    return Xenko.Graphics.GraphicsPlatform.OpenGLES;
                case PlatformType.Linux:
                    return Xenko.Graphics.GraphicsPlatform.OpenGL;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /*public static TextureQuality GetTextureQuality(this AssetCompilerContext context)
        {
            return context.PackageProperties.Get(XenkoConfig.TextureQuality);
        }

        public static GraphicsProfile GetGraphicsProfile(this AssetCompilerContext context)
        {
            var gameSettingsAsset = context.Package.Assets.Find(GameSettingsAsset.GameSettingsLocation);
            return gameSettingsAsset != null
                ? ((GameSettingsAsset)gameSettingsAsset.Asset).DefaultGraphicsProfile
                : GraphicsProfile.Level_10_0;
        }*/
    }
}