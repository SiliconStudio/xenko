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
    [Obsolete("This struct might be removed soon")]
    public struct AssetPart : IEquatable<AssetPart>
    {
        public AssetPart(Guid id, BasePart basePart = null)
        {
            Id = id;
            Base = basePart;
        }

        /// <summary>
        /// Asset identifier.
        /// </summary>
        public readonly Guid Id;

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public readonly BasePart Base;

        public bool Equals(AssetPart other)
        {
            return Id.Equals(other.Id) && Equals(Base?.BasePartAsset.Id, other.Base?.BasePartAsset.Id) && Equals(Base?.BasePartId, other.Base?.BasePartId) && Equals(Base?.InstanceId, other.Base?.InstanceId);
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
                if (Base != null)
                {
                    hashCode = (hashCode*397) ^ Base.BasePartAsset.Id.GetHashCode();
                    hashCode = (hashCode*397) ^ Base.BasePartId.GetHashCode();
                    hashCode = (hashCode*397) ^ Base.InstanceId.GetHashCode();
                }
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