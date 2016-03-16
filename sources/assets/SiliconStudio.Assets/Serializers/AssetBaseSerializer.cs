// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A Yaml Serializer for <see cref="AssetBase"/>. Because this type is immutable
    /// we need to implement a special serializer.
    /// </summary>
    [YamlSerializerFactory]
    internal class AssetBaseSerializer : ObjectSerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return CanVisit(typeDescriptor.Type) ? this : null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            objectContext.Instance = objectContext.SerializerContext.IsSerializing ? new AssetBaseMutable((AssetBase)objectContext.Instance) : new AssetBaseMutable();
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            objectContext.Instance = ((AssetBaseMutable)objectContext.Instance).ToAssetBase();
        }

        [NonIdentifiable]
        private class AssetBaseMutable
        {
            public AssetBaseMutable()
            {
            }

            public AssetBaseMutable(AssetBase item)
            {
                Location = item.Location;
                Asset = item.Asset;
            }

            [DataMember(1)]
            public UFile Location;

            [DataMember(2)]
            public Asset Asset;

            public AssetBase ToAssetBase()
            {
                return new AssetBase(Location, Asset);
            }
        }

        public bool CanVisit(Type type)
        {
            return type == typeof(AssetBase);
        }

        public void Visit(ref VisitorContext context)
        {
            // Only visit the instance without visiting childrens
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}