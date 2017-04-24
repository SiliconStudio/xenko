// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Assets;

namespace SiliconStudio.Core.Serialization.Contents
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
        AssetId Id { get; }

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
