using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// A serializer for the <see cref="Entity"/> type.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    public class EntitySerializer : ObjectSerializer
    {
        /// <inheritdoc/>
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return typeDescriptor.Type == typeof(Entity) ? this : null;
        }

        /// <inheritdoc/>
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            base.CreateOrTransformObject(ref objectContext);

            // When deserializing, we don't keep the TransformComponent created when the Entity is created
            if (!objectContext.SerializerContext.IsSerializing)
            {
                var entity = (Entity)objectContext.Instance;
                entity.Components.Clear();
            }
        }
    }
}
