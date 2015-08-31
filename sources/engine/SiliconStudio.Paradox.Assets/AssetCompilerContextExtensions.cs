// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    public static class AssetCompilerContextExtensions
    {
        private static readonly PropertyKey<GameSettingsAsset> GameSettingsAssetKey = new PropertyKey<GameSettingsAsset>("GameSettingsAsset", typeof(AssetCompilerContextExtensions));

        public static GameSettingsAsset GetGameSettingsAsset(this AssetCompilerContext context)
        {
            return context.Properties.Get(GameSettingsAssetKey);
        }

        /// <summary>
        /// Gets the <see cref="GameSettingsAsset"/> from either a global GameSettingsAssets or overriden by the executable Package being compiled.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assetItem">The asset item.</param>
        /// <returns>ColorSpace.</returns>
        public static GameSettingsAsset GetGameSettingsAsset(this AssetCompilerContext context, AssetItem assetItem)
        {
            var gameSettingsAssets = assetItem.Package.GetGameSettingsAsset();
            if (gameSettingsAssets == null)
            {
                var currentPackage = assetItem.Package.Session.CurrentPackage;
                if (currentPackage != null)
                {
                    gameSettingsAssets = currentPackage.GetGameSettingsAsset();
                }
            }

            if (gameSettingsAssets == null)
            {
                gameSettingsAssets = context.GetGameSettingsAsset();
            }

            return gameSettingsAssets;
        }

        /// <summary>
        /// Gets the color space from the current context (the color space could come from either a global GameSettingsAssets or overriden by the executable Package being compiled)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="assetItem">The asset item.</param>
        /// <returns>ColorSpace.</returns>
        public static ColorSpace GetColorSpace(this AssetCompilerContext context, AssetItem assetItem)
        {
            var settings = GetGameSettingsAsset(context, assetItem);
            // For profile below 9.3, we cannot use a linear workflow, so we stick to Gamma
            if (settings.DefaultGraphicsProfile < GraphicsProfile.Level_9_3)
            {
                return ColorSpace.Gamma;
            }
            return settings.ColorSpace;
        }

        public static void SetGameSettingsAsset(this AssetCompilerContext context, GameSettingsAsset gameSettingsAsset)
        {
            context.Properties.Set(GameSettingsAssetKey, gameSettingsAsset);
        }

        public static GraphicsPlatform GetGraphicsPlatform(this AssetCompilerContext context)
        {
            return context.Platform.GetDefaultGraphicsPlatform();
        }

        public static Paradox.Graphics.GraphicsPlatform GetDefaultGraphicsPlatform(this PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Windows:
                case PlatformType.WindowsPhone:
                case PlatformType.WindowsStore:
                case PlatformType.Windows10:
                    return Paradox.Graphics.GraphicsPlatform.Direct3D11;
                case PlatformType.Android:
                case PlatformType.iOS:
                    return Paradox.Graphics.GraphicsPlatform.OpenGLES;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /*public static TextureQuality GetTextureQuality(this AssetCompilerContext context)
        {
            return context.PackageProperties.Get(ParadoxConfig.TextureQuality);
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