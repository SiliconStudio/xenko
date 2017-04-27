// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
