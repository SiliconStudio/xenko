using System;
using System.ComponentModel;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SerializerContext = SiliconStudio.Core.Serialization.SerializerContext;

namespace SiliconStudio.Assets
{
    [DataContract]
    [DataSerializer(typeof(BasePartDataSerializer))]
    public class BasePart
    {
        public BasePart(AssetReference basePartAsset, Guid basePartId, Guid instanceId)
        {
            if (basePartAsset == null) throw new ArgumentNullException(nameof(basePartAsset));
            if (basePartId == Guid.Empty) throw new ArgumentException(nameof(basePartAsset));
            if (instanceId == Guid.Empty) throw new ArgumentException(nameof(basePartAsset));
            BasePartAsset = basePartAsset;
            BasePartId = basePartId;
            InstanceId = instanceId;
        }

        [DataMember(10)]
        public AssetReference BasePartAsset { get; }

        [DataMember(20)]
        public Guid BasePartId { get; }

        [DataMember(30)]
        public Guid InstanceId { get; }

        public IIdentifiable ResolvePart(PackageSession session)
        {
            var assetItem = session.FindAsset(BasePartAsset.Id);
            var asset = assetItem?.Asset as AssetComposite;
            return asset?.FindPart(BasePartId);
        }
    }

    public class BasePartDataSerializer : DataSerializer<BasePart>
    {
        /// <inheritdoc/>
        public override void Serialize(ref BasePart basePart, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(basePart.BasePartAsset);
                stream.Write(basePart.BasePartId);
                stream.Write(basePart.InstanceId);
            }
            else
            {
                var asset = stream.Read<AssetReference>();
                var baseId = stream.Read<Guid>();
                var instanceId = stream.Read<Guid>();
                basePart = new BasePart(asset, baseId, instanceId);
            }
        }
    }

    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class BasePartYamlSerializer : ObjectSerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(Core.Yaml.Serialization.SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return CanVisit(typeDescriptor.Type) ? this : null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            objectContext.Instance = objectContext.SerializerContext.IsSerializing ? new BasePartMutable((BasePart)objectContext.Instance) : new BasePartMutable();
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            objectContext.Instance = ((BasePartMutable)objectContext.Instance).ToBasePart();
        }

        private class BasePartMutable
        {
            public BasePartMutable()
            {
            }

            public BasePartMutable(BasePart item)
            {
                BasePartAsset = item.BasePartAsset;
                BasePartId = item.BasePartId;
                InstanceId = item.InstanceId;
            }

            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            [DataMember(10)]
            public AssetReference BasePartAsset;

            [DataMember(20)]
            public Guid BasePartId;

            [DataMember(30)]
            public Guid InstanceId;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
            // ReSharper restore MemberCanBePrivate.Local

            public BasePart ToBasePart()
            {
                return new BasePart(BasePartAsset, BasePartId, InstanceId);
            }
        }

        public bool CanVisit(Type type)
        {
            return type == typeof(BasePart);
        }

        public void Visit(ref VisitorContext context)
        {
            context.Visitor.VisitObject(context.Instance, context.Descriptor, true);
        }
    }

}
