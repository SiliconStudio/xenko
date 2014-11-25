using System;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Specifies which class members should be used as comparison key when doing a collection diff.
    /// </summary>
    /// <remarks>This can be applied on multiple members in a given class.</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DiffCompareKeyAttribute : DiffAttributeBase
    {
    }
}