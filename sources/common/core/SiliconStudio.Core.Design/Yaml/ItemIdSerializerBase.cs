using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A base class to serialize <see cref="ItemId"/>.
    /// </summary>
    public abstract class ItemIdSerializerBase : AssetScalarSerializerBase
    {
        /// <summary>
        /// A key used in properties of serialization contexts to notify whether an override flag should be appened when serializing the related <see cref="ItemId"/>.
        /// </summary>
        public static PropertyKey<string> OverrideInfoKey = new PropertyKey<string>("OverrideInfo", typeof(ItemIdSerializer));

        /// <inheritdoc/>
        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            string overrideInfo;
            if (objectContext.SerializerContext.Properties.TryGetValue(OverrideInfoKey, out overrideInfo))
            {
                scalar.RenderedValue += overrideInfo;
                objectContext.SerializerContext.Properties.Remove(OverrideInfoKey);
            }

            base.WriteScalar(ref objectContext, scalar);
        }
    }
}
