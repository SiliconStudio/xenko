// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace SiliconStudio.AssemblyProcessor.Serializers
{
    class CecilComplexClassSerializerProcessor : ICecilSerializerProcessor
    {
        public void ProcessSerializers(CecilSerializerContext context)
        {
            foreach (var type in context.Assembly.MainModule.GetAllTypes())
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
