// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Generates enum serializer type from a given enum type.
    /// </summary>
    public class CecilEnumSerializerFactory : ICecilSerializerFactory
    {
        private readonly TypeReference genericEnumSerializerType;

        public CecilEnumSerializerFactory(TypeReference genericEnumSerializerType)
        {
            this.genericEnumSerializerType = genericEnumSerializerType;
        }

        public TypeReference GetSerializer(TypeReference objectType)
        {
            var resolvedObjectType = objectType.Resolve();
            if (resolvedObjectType != null && resolvedObjectType.IsEnum)
            {
                return genericEnumSerializerType.MakeGenericType(objectType);
            }

            return null;
        }
    }
}