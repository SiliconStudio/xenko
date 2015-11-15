using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.SpriteStudio.Runtime;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract("SpriteStudioAnimationAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioAnimationAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetDescription(".xkss4a;.pdxss4a", false)] // A description used to display in the asset editor
    [ObjectFactory(typeof(SpriteStudioAnimationAssetFactory))]
    [Display("Sprite Studio Animation")]
    public class SpriteStudioAnimationAsset : AssetImportTracked
    {
        [DataMember(1)]
        public AnimationRepeatMode RepeatMode { get; set; } = AnimationRepeatMode.LoopInfinite;

        [DataMember(2)]
        [Display(Browsable = false)]
        [DiffMember(Weight = 100)] // Because AnimationName is like a key, we use a high weight in order to match asset more accurately
        public string AnimationName;

        private class SpriteStudioAnimationAssetFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new SpriteStudioAnimationAsset();
            }
        }
    }
}