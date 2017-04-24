// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor.Serializers
{
    class PropertyKeySerializerProcessor : ICecilSerializerProcessor
    {
        public void ProcessSerializers(CecilSerializerContext context)
        {
            // Iterate over each static member of type PropertyKey<> or ParameterKey<>
            foreach (var type in context.Assembly.EnumerateTypes())
            {
                foreach (var member in type.Fields)
                {
                    if (!member.IsStatic || !member.IsPublic)
                        continue;

                    if (member.FieldType.Name == "PropertyKey`1"
                        || member.FieldType.Name == "ParameterKey`1"
                        || member.FieldType.Name == "ValueParameterKey`1"
                        || member.FieldType.Name == "ObjectParameterKey`1"
                        || member.FieldType.Name == "PermutationParameterKey`1")
                    {
                        context.GenerateSerializer(member.FieldType);

                        var genericType = (GenericInstanceType)member.FieldType;

                        // Also generate serializer for embedded type
                        context.GenerateSerializer(genericType.GenericArguments[0]);
                    }
                }
            }
        }
    }
}
