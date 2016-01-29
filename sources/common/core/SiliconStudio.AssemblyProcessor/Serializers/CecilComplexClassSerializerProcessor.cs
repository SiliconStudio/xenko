// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor.Serializers
{
    class CecilComplexClassSerializerProcessor : ICecilSerializerProcessor
    {
        public void ProcessSerializers(CecilSerializerContext context)
        {
            foreach (var type in context.Assembly.EnumerateTypes())
            {
                // Force generation of serializers (complex types, etc...)
                // Check complex type definitions
                ProcessType(context, type);
            }
        }

        private static void ProcessType(CecilSerializerContext context, TypeDefinition type)
        {
            CecilSerializerContext.SerializableTypeInfo serializableTypeInfo;
            if (!context.SerializableTypes.TryGetSerializableTypeInfo(type, false, out serializableTypeInfo)
                && !context.SerializableTypes.TryGetSerializableTypeInfo(type, true, out serializableTypeInfo))
            {
                context.FindSerializerInfo(type, false);
            }

            if (type.HasNestedTypes)
            {
                foreach (var nestedType in type.NestedTypes)
                {
                    ProcessType(context, nestedType);
                }
            }
        }
    }
}