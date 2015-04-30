using System;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Specifies that <see cref="Diff3Node.Asset2Node"/> should take priority when doing a diff.
    /// </summary>
    /// <remarks>This currently have effect on value and collections.</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DiffUseAsset2Attribute : DiffUseSpecificAssetAttribute
    {
    }
}