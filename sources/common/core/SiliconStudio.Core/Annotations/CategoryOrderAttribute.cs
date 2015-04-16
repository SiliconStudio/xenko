using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attribute allows to associate an <see cref="Order"/> value to a category name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public sealed class CategoryOrderAttribute : Attribute
    {
        public CategoryOrderAttribute(int order, string name)
        {
            Order = order;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order value of the category.
        /// </summary>
        public int Order { get; set; }
    }
}