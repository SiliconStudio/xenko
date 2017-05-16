// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Interface of a factory that can create instances of a type.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new instance of a type.
        /// </summary>
        /// <param name="type">The type of the instance to create.</param>
        /// <returns>A new default instance of a type.</returns>
        object New(Type type);
    }
}
