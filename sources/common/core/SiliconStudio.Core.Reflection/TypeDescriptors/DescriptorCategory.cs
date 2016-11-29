// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A category used by <see cref="ITypeDescriptorBase"/>.
    /// </summary>
    public enum DescriptorCategory
    {
        /// <summary>
        /// A primitive.
        /// </summary>
        Primitive,

        /// <summary>
        /// A collection.
        /// </summary>
        Collection,

        /// <summary>
        /// An array
        /// </summary>
        Array,

        /// <summary>
        /// A dictionary
        /// </summary>
        Dictionary,

        /// <summary>
        /// An object
        /// </summary>
        Object,

        /// <summary>
        /// A nullable value
        /// </summary>
        Nullable,

        /// <summary>
        /// A custom descriptor.
        /// </summary>
        Custom
    }
}
