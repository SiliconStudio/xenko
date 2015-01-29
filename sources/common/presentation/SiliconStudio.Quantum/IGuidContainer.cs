// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Base interface for Guid containers, object that can store a unique identifier for a collection of object.
    /// </summary>
    /// <remarks>Specialization of this interface will usually hold references on objects until they are unregistered or until the container is cleared.</remarks>
    public interface IGuidContainer
    {
        /// <summary>
        /// Gets or or create a <see cref="Guid"/> for a given object. If the object is <c>null</c>, a new Guid will be returned.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="Guid"/> associated to the given object, or a newly registered <see cref="Guid"/> if the object was not previously registered.</returns>
        Guid GetOrCreateGuid(object obj);

        /// <summary>
        /// Gets the <see cref="Guid"/> for a given object, if available.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="Guid"/> associated to the given object, or <see cref="Guid.Empty"/> if the object was not previously registered.</returns>
        Guid GetGuid(object obj);

        /// <summary>
        /// Register the given <see cref="Guid"/> to the given object. If a <see cref="Guid"/> is already associated to this object, it is replaced by the new one.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> to register.</param>
        /// <param name="obj">The object to register.</param>
        void RegisterGuid(Guid guid, object obj);

        /// <summary>
        /// Clear the <see cref="IGuidContainer"/>, removing everything it references.
        /// </summary>
        void Clear();
    }
}