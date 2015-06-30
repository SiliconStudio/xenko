using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attribute indicates that the associated type cannot be instanced in the property grid
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NonInstantiableAttribute : Attribute
    {
    }
}