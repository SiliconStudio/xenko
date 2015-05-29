// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    internal static class AssetCompilerContextExtensions
    {
        public static GraphicsPlatform GetGraphicsPlatform(this AssetCompilerContext context)
        {
            return context.Properties.Get(ParadoxConfig.GraphicsPlatform);
        }

        public static TextureQuality GetTextureQuality(this AssetCompilerContext context)
        {
            return context.Properties.Get(ParadoxConfig.TextureQuality);
        }

        public static GraphicsProfile GetGraphicsProfile(this AssetCompilerContext context)
        {
            return context.Properties.Get(GameSettingsAsset.DefaultGraphicsProfile);
        }
    }
}