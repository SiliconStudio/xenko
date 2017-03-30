using System;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    /// <summary>
    /// Specifies that this field or property should be set if a compatible object is dropped on its containing <see cref="Block"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BlockDropTargetAttribute : Attribute
    {
    }
}