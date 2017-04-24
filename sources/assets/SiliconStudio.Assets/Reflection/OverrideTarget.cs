// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Describes what is targeted by an override information.
    /// </summary>
    public enum OverrideTarget
    {
        /// <summary>
        /// The content itself.
        /// </summary>
        Content,
        /// <summary>
        /// An item of the content if it's a collection, or a value of the content if it's a dictionary.
        /// </summary>
        Item,
        /// <summary>
        /// A key of the content. This is valid only for dictionary.
        /// </summary>
        Key
    }
}
