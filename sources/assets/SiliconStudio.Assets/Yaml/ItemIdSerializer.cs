using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="ItemId"/> without associated data.
    /// </summary>
    [YamlSerializerFactory("Assets")] // TODO: use YamlAssetProfile.Name
    internal class ItemIdSerializer : ItemIdSerializerBase
    {
        /// <inheritdoc/>
        public override bool CanVisit(Type type)
        {
            return type == typeof(ItemId);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            ObjectId id;
            ObjectId.TryParse(fromScalar.Value, out id);
            return new ItemId(id);
        }
    }
}
