// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
