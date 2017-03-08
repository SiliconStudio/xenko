using System;
using SiliconStudio.Assets;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Represents the absolute identifier of an identifiable object in an asset.
    /// </summary>
    [DataContract("AbsoluteId")]
    public struct AbsoluteId : IEquatable<AbsoluteId>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AbsoluteId"/>.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="guid"></param>
        /// <exception cref="ArgumentException"><paramref name="assetId"/> and <paramref name="guid"/> cannot both be empty.</exception>
        public AbsoluteId(AssetId assetId, Guid guid)
        {
            if (assetId == AssetId.Empty && guid == Guid.Empty)
                throw new ArgumentException($"{nameof(assetId)} and {nameof(guid)} cannot both be empty.");

            AssetId = assetId;
            Guid = guid;
        }

        public AssetId AssetId { get; }

        public Guid Guid { get; }

        /// <inheritdoc />
        public static bool operator ==(AbsoluteId left, AbsoluteId right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(AbsoluteId left, AbsoluteId right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public bool Equals(AbsoluteId other)
        {
            return AssetId.Equals(other.AssetId) && Guid.Equals(other.Guid);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AbsoluteId && Equals((AbsoluteId)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (AssetId.GetHashCode() * 397) ^ Guid.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{AssetId}/{Guid}";
        }
    }
}
