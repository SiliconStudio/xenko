// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Gives the required generic serializer for a given type.
    /// This is useful for generation of serialization assembly, when AOT is performed (all generic serializers must be available).
    /// </summary>
    public interface ICecilSerializerFactory
    {
        /// <summary>
        /// Gets the serializer type from a given object type.
        /// </summary>
        /// <param name="objectType">Type of the object to serialize.</param>
        /// <returns></returns>
        TypeReference GetSerializer(TypeReference objectType);
    }
}