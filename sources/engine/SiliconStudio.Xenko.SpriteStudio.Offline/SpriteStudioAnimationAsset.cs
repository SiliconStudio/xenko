using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Animations;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract("SpriteStudioAnimationAsset")] // Name of the Asset serialized in YAML
    [AssetCompiler(typeof(SpriteStudioAnimationAssetCompiler))] // The compiler used to transform this asset to RangeValues
    [AssetContentType(typeof(AnimationClip))]
    [AssetDescription(".xkss4a;.pdxss4a")] // A description used to display in the asset editor
    [Display("Sprite Studio Animation")]
    public class SpriteStudioAnimationAsset : AssetWithSource
    {
        [DataMember(1)]
        [DefaultValue(AnimationRepeatMode.LoopInfinite)]
        public AnimationRepeatMode RepeatMode { get; set; } = AnimationRepeatMode.LoopInfinite;

        [DataMember(2)]
        [Display(Browsable = false)]
        [DefaultValue("")]
        public string AnimationName { get; set; } = "";
    }
}
