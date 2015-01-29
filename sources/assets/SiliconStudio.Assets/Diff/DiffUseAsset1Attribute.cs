using System;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Specifies that <see cref="Diff3Node.Asset1Node"/> should take priority when doing a diff.
    /// </summary>
    /// <remarks>This currently have effect on value and collections.</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DiffUseAsset1Attribute : DiffUseSpecificAssetAttribute
    {
    }
}