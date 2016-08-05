// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Adds initialization feature to a <see cref="DataSerializer"/>.
    /// </summary>
    public interface IDataSerializerInitializer
    {
        /// <summary>
        /// Initializes the specified serializer.
        /// </summary>
        /// <remarks>This method should be thread-safe and OK to call multiple times.</remarks>
        /// <param name="serializerSelector">The serializer.</param>
        void Initialize(SerializerSelector serializerSelector);
    }
}