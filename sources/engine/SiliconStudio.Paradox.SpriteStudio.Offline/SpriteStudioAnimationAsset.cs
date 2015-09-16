using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Animations;
using SiliconStudio.Paradox.SpriteStudio.Runtime;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract("SpriteStudioAnimationAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioAnimationAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".pdxss4a", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioAnimationAssetFactory))]
    [ThumbnailCompiler("SiliconStudio.Paradox.GameStudio.Plugin.ThumbnailCompilers.SpriteStudioAnimationThumbnailCompiler, SiliconStudio.Paradox.GameStudio.Plugin")] // TODO: Obsolete
    [Display("Sprite Studio Animation")]
    public class SpriteStudioAnimationAsset : AssetImportTracked
    {
        public SpriteStudioAnimationAsset()
        {
        }

        [DataMember(1)]
        public AnimationRepeatMode RepeatMode { get; set; } = AnimationRepeatMode.LoopInfinite;

        private class SpriteStudioAnimationAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioAnimationAsset();
            }
        }
    }
}