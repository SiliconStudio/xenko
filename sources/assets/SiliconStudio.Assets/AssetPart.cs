// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
        public AssetPart(Guid partId, BasePart basePart, Action<BasePart> baseUpdater)
        {
            if (baseUpdater == null) throw new ArgumentNullException(nameof(baseUpdater));
            if (partId == Guid.Empty) throw new ArgumentException(@"A part Id cannot be empty.", nameof(partId));
            PartId = partId;
            Base = basePart;
            this.baseUpdater = baseUpdater;
        }

        /// <summary>
        /// Asset identifier.
        /// </summary>
        public readonly Guid PartId;

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public readonly BasePart Base;

        private readonly Action<BasePart> baseUpdater;

        public void UpdateBase(BasePart newBase)
        {
            baseUpdater(newBase);
        }

        public bool Equals(AssetPart other)
        {
            return PartId.Equals(other.PartId) && Equals(Base?.BasePartAsset.Id, other.Base?.BasePartAsset.Id) && Equals(Base?.BasePartId, other.Base?.BasePartId) && Equals(Base?.InstanceId, other.Base?.InstanceId);
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
                var hashCode = PartId.GetHashCode();
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
