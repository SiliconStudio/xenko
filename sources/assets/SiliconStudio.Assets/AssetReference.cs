// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset reference.
    /// </summary>
    [DataContract("aref")]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(AssetReferenceDataSerializer))]
    public class AssetReference : IReference, IEquatable<AssetReference>
    {
        private readonly UFile location;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReference"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the asset.</param>
        /// <param name="location">The location.</param>
        public AssetReference(AssetId id, UFile location)
        {
            this.location = location;
            Id = id;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the reference asset.
        /// </summary>
        /// <value>The unique identifier of the reference asset..</value>
        [DataMember(10)]
        public AssetId Id { get; }

        /// <summary>
        /// Gets or sets the location of the asset.
        /// </summary>
        /// <value>The location.</value>
        [DataMember(20)]
        public string Location => location;

        public bool Equals(AssetReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(location, other.location) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var assetReference = obj as AssetReference;
            return assetReference != null && Equals(assetReference);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((location != null ? location.GetHashCode() : 0)*397) ^ Id.GetHashCode();
            }
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(AssetReference left, AssetReference right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(AssetReference left, AssetReference right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            // WARNING: This should not be modified as it is used for serializing
            return $"{Id}:{location}";
        }

        /// <summary>
        /// Tries to parse an asset reference in the format "GUID:Location".
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        public static AssetReference New(AssetId id, UFile location)
        {
            return new AssetReference(id, location);            
        }

        /// <summary>
        /// Tries to parse an asset reference in the format "[GUID/]GUID:Location". The first GUID is optional and is used to store the ID of the reference.
        /// </summary>
        /// <param name="assetReferenceText">The asset reference.</param>
        /// <param name="id">The unique identifier of asset pointed by this reference.</param>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">assetReferenceText</exception>
        public static bool TryParse(string assetReferenceText, out AssetId id, out UFile location)
        {
            if (assetReferenceText == null) throw new ArgumentNullException(nameof(assetReferenceText));

            id = AssetId.Empty;
            location = null;
            int indexFirstSlash = assetReferenceText.IndexOf('/');
            int indexBeforelocation = assetReferenceText.IndexOf(':');
            if (indexBeforelocation < 0)
            {
                return false;
            }
            int startNextGuid = 0;
            if (indexFirstSlash > 0 && indexFirstSlash < indexBeforelocation)
            {
                startNextGuid = indexFirstSlash + 1;
            }

            if (!AssetId.TryParse(assetReferenceText.Substring(startNextGuid, indexBeforelocation - startNextGuid), out id))
            {
                return false;
            }

            location = new UFile(assetReferenceText.Substring(indexBeforelocation + 1));

            return true;
        }

        /// <summary>
        /// Tries to parse an asset reference in the format "GUID:Location".
        /// </summary>
        /// <param name="assetReferenceText">The asset reference.</param>
        /// <param name="assetReference">The reference.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        public static bool TryParse(string assetReferenceText, out AssetReference assetReference)
        {
            if (assetReferenceText == null) throw new ArgumentNullException(nameof(assetReferenceText));

            assetReference = null;
            AssetId assetId;
            UFile location;
            if (!TryParse(assetReferenceText, out assetId, out location))
            {
                return false;
            }
            assetReference = New(assetId, location);
            return true;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="AssetReference"/>
    /// </summary>
    public static class AssetReferenceExtensions
    {
        /// <summary>
        /// Determines whether the specified asset reference has location. If the reference is null, return <c>false</c>.
        /// </summary>
        /// <param name="assetReference">The asset reference.</param>
        /// <returns><c>true</c> if the specified asset reference has location; otherwise, <c>false</c>.</returns>
        public static bool HasLocation(this AssetReference assetReference)
        {
            return assetReference != null && assetReference.Location != null;
        }
    }
}
