using System;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    /// <summary>
    /// <see cref="Block.Title"/> need to be recomputed if a member with this attribute is changed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RegenerateTitleAttribute : Attribute
    {

    }
}