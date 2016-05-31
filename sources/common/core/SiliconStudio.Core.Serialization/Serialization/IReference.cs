// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// An interface that provides a reference to an object identified by a <see cref="Guid"/> and a location.
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// Gets the asset unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
        Guid Id { get; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        string Location { get; }
    }


    /// <summary>
    /// A typed <see cref="IReference"/>
    /// </summary>
    public interface ITypedReference : IReference
    {
        /// <summary>
        /// Gets the type of this content reference.
        /// </summary>
        /// <value>The type.</value>
        Type Type { get; }
    }
}
