// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Serialization.Serializers
{
    public class PropertyInfoSerializer : DataSerializer<PropertyInfo>
    {
        public override void Serialize(ref PropertyInfo propertyInfo, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(propertyInfo.DeclaringType.AssemblyQualifiedName);
                stream.Write(propertyInfo.Name);
            }
            else
            {
                var declaringTypeName = stream.ReadString();
                var propertyName = stream.ReadString();

                var ownerType = AssemblyRegistry.GetType(declaringTypeName);
                if (ownerType == null)
                    throw new InvalidOperationException("Could not find the appropriate type.");

                propertyInfo = ownerType.GetTypeInfo().GetDeclaredProperty(propertyName);
            }
        }
    }
}