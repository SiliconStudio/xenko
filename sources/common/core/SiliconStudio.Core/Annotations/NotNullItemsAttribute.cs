using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// Indicates that the items of the marked collection could never be <c>null</c>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter |
        AttributeTargets.Property | AttributeTargets.Delegate |
        AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotNullItemsAttribute : Attribute { }
}