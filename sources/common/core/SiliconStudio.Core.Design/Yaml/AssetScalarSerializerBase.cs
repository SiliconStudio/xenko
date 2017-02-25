// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    public abstract class AssetScalarSerializerBase : ScalarSerializerBase, IYamlSerializableFactory, IDataCustomVisitor
    {
        public IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return CanVisit(typeDescriptor.Type) ? this : null;
        }

        public abstract bool CanVisit(Type type);

        public virtual void Visit(ref VisitorContext context)
        {
            // For a scalar object, we don't visit its members
            // But we do still visit the instance (either struct or class)
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}