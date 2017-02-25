using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attribute notifies that the attached member cannot be overridden.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class NonOverridableAttribute : Attribute
    {
        
    }
}