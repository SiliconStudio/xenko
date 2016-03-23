using System;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="MaterialNull"/>
    /// </summary>
    [YamlSerializerFactory]
    internal class MaterialNullSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(MaterialNull).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            Guid id;
            if (!Guid.TryParse(fromScalar.Value, out id))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, $"Unable to parse id [{fromScalar.Value}]");
            }
            var materialNull = new MaterialNull();
            IdentifiableHelper.SetId(materialNull, id);
            return materialNull;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var materialNull = ((MaterialNull)objectContext.Instance);
            return IdentifiableHelper.GetId(materialNull).ToString();
        }
    }
}