using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Xenko.Assets.Entities
{
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    public sealed class EntityComponentReferenceSerializer : ScalarOrObjectSerializer
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(EntityComponentReference);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid entityGuid, componentGuid;

            if (!TryParse(fromScalar.Value, out entityGuid, out componentGuid))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode entity component reference [{0}]. Expecting format ENTITY_GUID/COMPONENT_GUID".ToFormat(fromScalar.Value));
            }

            var result = context.Instance as EntityComponentReference ?? (EntityComponentReference)(context.Instance = new EntityComponentReference());
            result.Entity = new EntityComponentReference.EntityReference { Id = entityGuid };
            result.Id = componentGuid;
            return result;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var entityComponentReference = (EntityComponentReference)objectContext.Instance;
            return $"{entityComponentReference.Entity.Id}/{entityComponentReference.Id}";
        }

        private bool TryParse(string entityComponentReference, out Guid entityGuid, out Guid componentGuid)
        {
            int indexFirstSlash = entityComponentReference.IndexOf('/');
            if (indexFirstSlash == -1)
            {
                entityGuid = Guid.Empty;
                componentGuid = Guid.Empty;
                return false;
            }

            if (!Guid.TryParse(entityComponentReference.Substring(0, indexFirstSlash), out entityGuid))
            {
                componentGuid = Guid.Empty;
                return false;
            }

            if (!Guid.TryParse(entityComponentReference.Substring(indexFirstSlash + 1), out componentGuid))
            {
                return false;
            }

            return true;
        }
    }
}
