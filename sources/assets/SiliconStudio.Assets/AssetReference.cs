// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An asset reference.
    /// </summary>
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [DataSerializer(typeof(AssetReferenceDataSerializer))]
    public sealed class AssetReference : IContentReference, IEquatable<AssetReference>
    {
        private readonly UFile location;
        private readonly Guid id;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReference"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the asset.</param>
        /// <param name="location">The location.</param>
        public AssetReference(Guid id, UFile location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            this.location = location;
            this.id = id;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the reference asset.
        /// </summary>
        /// <value>The unique identifier of the reference asset..</value>
        [DataMember(10)]
        public Guid Id
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// Gets or sets the location of the asset.
        /// </summary>
        /// <value>The location.</value>
        [DataMember(20)]
        public string Location
        {
            get
            {
                return location;
            }
        }

        public bool Equals(AssetReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(location, other.location) && id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is AssetReference && Equals((AssetReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((location != null ? location.GetHashCode() : 0)*397) ^ id.GetHashCode();
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
            return string.Format("{0}:{1}", id, location);
        }

        /// <summary>
        /// Tries to parse an asset reference in the format "GUID:Location".
        /// </summary>
        /// <param name="assetReferenceText">The asset reference.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">assetReferenceText</exception>
        public static bool TryParse(string assetReferenceText, out Guid guid, out UFile location)
        {
            if (assetReferenceText == null) throw new ArgumentNullException("assetReferenceText");

            guid = Guid.Empty;
            location = null;
            int indexOf = assetReferenceText.IndexOf(':');
            if (indexOf < 0)
            {
                return false;
            }
            if (!Guid.TryParse(assetReferenceText.Substring(0, indexOf), out guid))
            {
                return false;
            }
            location = new UFile(assetReferenceText.Substring(indexOf + 1));

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
            if (assetReferenceText == null) throw new ArgumentNullException("assetReferenceText");

            assetReference = null;
            Guid guid;
            UFile location;
            if (!TryParse(assetReferenceText, out guid, out location))
            {
                return false;
            }
            assetReference = new AssetReference(guid, location);
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