// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Enumerates required subtypes the given serializer will use internally.
    /// This is useful for generation of serialization assembly, when AOT is performed (all generic serializers must be available).
    /// </summary>
    public interface ICecilSerializerDependency
    {
        /// <summary>
        /// Enumerates the types this serializer requires.
        /// </summary>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns></returns>
        IEnumerable<TypeReference> EnumerateSubTypesFromSerializer(TypeReference serializerType);
    }
}