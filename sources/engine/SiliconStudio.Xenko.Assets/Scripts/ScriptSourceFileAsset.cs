using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract("ScriptSourceFileAsset")]
    [AssetDescription(Extension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [Display(95, "Script Source Code")]
    public sealed class ScriptSourceFileAsset : ProjectSourceCodeAsset
    {
        public const string Extension = ".cs";
    }
}
