// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A part asset contained by an asset that is <see cref="IAssetComposite"/>.
    /// </summary>
    [DataContract("AssetPart")]
    public struct AssetPart : IEquatable<AssetPart>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetPart"/> with a base.
        /// </summary>
        /// <param name="id">The asset identifier</param>
        /// <param name="baseId">The base asset identifier</param>
        /// <param name="basePartInstanceId">The identifier of the instance group used in a base composition</param>
        public AssetPart(Guid id, Guid? baseId = null, Guid? basePartInstanceId = null)
        {
            Id = id;
            BaseId = baseId;
            BasePartInstanceId = basePartInstanceId;
        }

        /// <summary>
        /// Asset identifier.
        /// </summary>
        public readonly Guid Id;

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public readonly Guid? BaseId;

        /// <summary>
        /// Identifier used for a base part group.
        /// </summary>
        public readonly Guid? BasePartInstanceId;

        public bool Equals(AssetPart other)
        {
            return Id.Equals(other.Id) && BaseId.Equals(other.BaseId) && BasePartInstanceId.Equals(other.BasePartInstanceId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetPart && Equals((AssetPart)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ BaseId.GetHashCode();
                hashCode = (hashCode*397) ^ BasePartInstanceId.GetHashCode();
                return hashCode;
            }
        }
        public static bool operator ==(AssetPart left, AssetPart right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetPart left, AssetPart right)
        {
            return !left.Equals(right);
        }
    }
}