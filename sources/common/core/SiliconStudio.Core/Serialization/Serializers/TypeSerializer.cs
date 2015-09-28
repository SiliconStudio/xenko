// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Serialization.Serializers
{
    [DataSerializerGlobal(typeof(TypeSerializer))]
    public class TypeSerializer : DataSerializer<Type>
    {
        public override void Serialize(ref Type type, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(type.AssemblyQualifiedName);
            }
            else
            {
                var typeName = stream.ReadString();
                type = AssemblyRegistry.GetType(typeName);
            }
        }
    }
}