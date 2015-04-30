// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Interface to create default instance of a type.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new default instance of a type.
        /// </summary>
        /// <param name="type">Type of the instance to create</param>
        /// <returns>A new default instance of a type.</returns>
        object New(Type type);
    }
}