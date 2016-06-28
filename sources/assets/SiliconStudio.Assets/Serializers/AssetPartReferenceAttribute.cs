using System;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// An attribute indicating which types are part types in an <see cref="AssetComposite"/> and how to serialize references between different parts.
    /// </summary>
    /// <remarks>
    /// When visiting an <see cref="AssetComposite"/> for serialization, part objects that are referenced, directly or via nested members, by another
    /// part object will be serialized as a reference. The type to use to serialize the reference is defined by <see cref="ReferenceType"/>.
    /// A part object will be fully serialized (not as reference) only if it is not contained in another part object. However, sometimes part objects
    /// of a certain type might be the natural containers of part objects of another type. Each type contained in the <see cref="ContainedTypes"/> collection
    /// will therefore be fully serialized, only if they are contained in a part of the type indicated by the <see cref="ReferenceableType"/> type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AssetPartReferenceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPartReferenceAttribute"/>.
        /// </summary>
        /// <param name="referenceableType">The type of asset part that can possibly by serialized as a reference.</param>
        /// <param name="containedTypes">The collection of asset part types that are naturally contained in the type indicated by <paramref name="referenceableType"/>.</param>
        public AssetPartReferenceAttribute(Type referenceableType, params Type[] containedTypes)
        {
            ReferenceableType = referenceableType;
            ContainedTypes = containedTypes ?? new Type[0];
        }

        /// <summary>
        /// Gets the type of asset part that will be serialized as a reference when contained in any other part.
        /// </summary>
        public Type ReferenceableType { get; }

        /// <summary>
        /// Gets the types of asset part that will still be fully serialized if contained in a part of the <see cref="ReferenceableType"/> type.
        /// </summary>
        public Type[] ContainedTypes { get; }

        /// <summary>
        /// Gets or sets the type to use to create references to asset parts of the <see cref="ReferenceableType"/> in order to serialize references.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="IdentifiableAssetPartReference"/>.
        /// </remarks>
        public Type ReferenceType { get; set; } = typeof(IdentifiableAssetPartReference);
    }
}