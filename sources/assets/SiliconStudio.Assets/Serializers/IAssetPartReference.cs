using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// An interface representing a reference to an asset part that is used for serialization.
    /// </summary>
    public interface IAssetPartReference
    {
        /// <summary>
        /// Gets or sets the actual type of object that is being deserialized.
        /// </summary>
        /// <remarks>
        /// This property is transient and used only during serialization. Therefore, implementations should have the <see cref="DataMemberIgnoreAttribute"/> set on this property.
        /// </remarks>
        Type InstanceType { get; set; }

        /// <summary>
        /// Fills properties of this object from the actual asset part being referenced.
        /// </summary>
        /// <param name="assetPart">The actual asset part being referenced.</param>
        void FillFromPart(object assetPart);

        /// <summary>
        /// Generates a proxy asset part from the information contained in this instance.
        /// </summary>
        /// <param name="partType">The type of asset part to generate.</param>
        /// <returns>A proxy asset part built from this instance.</returns>
        /// <remarks>
        /// Proxy asset parts should be resolved to the actual corresponding asset part in the <see cref="AssetComposite.FixupPartReferences"/> method.
        /// This method is invoked at the end of the deserialization.
        /// </remarks>
        object GenerateProxyPart(Type partType);
    }
}