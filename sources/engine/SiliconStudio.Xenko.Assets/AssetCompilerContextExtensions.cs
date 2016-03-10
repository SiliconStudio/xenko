// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Settings;
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
            var settings = context.GetGameSettingsAsset().Get<RenderingSettings>(context.Platform);
            return settings.ColorSpace;
        }

        public static void SetGameSettingsAsset(this AssetCompilerContext context, GameSettingsAsset gameSettingsAsset)
        {
            context.Properties.Set(GameSettingsAssetKey, gameSettingsAsset);
        }

        public static GraphicsPlatform GetGraphicsPlatform(this AssetCompilerContext context, Package package)
        {
            var buildProfile = package.Profiles.FirstOrDefault(pair => pair.Name == context.Profile);
            if (buildProfile == null)
            {
                return context.Platform.GetDefaultGraphicsPlatform();
            }

            var settings = package.GetGameSettingsAsset();
            return RenderingSettings.GetGraphicsPlatform(context.Platform, settings.Get<RenderingSettings>(context.Profile).PreferredGraphicsPlatform);
        }

        public static GraphicsPlatform GetDefaultGraphicsPlatform(this PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                case PlatformType.WindowsPhone:
                case PlatformType.WindowsStore:
                case PlatformType.Windows10:
                    return GraphicsPlatform.Direct3D11;
                case PlatformType.Android:
                case PlatformType.iOS:
                    return GraphicsPlatform.OpenGLES;
#if SILICONSTUDIO_RUNTIME_CORECLR
                case PlatformType.Linux:
                    return GraphicsPlatform.OpenGL;
#endif
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