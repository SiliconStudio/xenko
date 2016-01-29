// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Generates array serializer type from a given array type.
    /// </summary>
    public class CecilArraySerializerFactory : ICecilSerializerFactory
    {
        private readonly TypeReference genericArraySerializerType;

        public CecilArraySerializerFactory(TypeReference genericArraySerializerType)
        {
            this.genericArraySerializerType = genericArraySerializerType;
        }

        public TypeReference GetSerializer(TypeReference objectType)
        {
            if (objectType.IsArray)
            {
                return genericArraySerializerType.MakeGenericType(((ArrayType)objectType).ElementType);
            }

            return null;
        }
    }
}