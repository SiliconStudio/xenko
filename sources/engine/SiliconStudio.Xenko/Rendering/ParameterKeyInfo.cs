using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract]
    public struct ParameterKeyInfo
    {
        // Common
        public ParameterKey Key;

        // Values
        public int Offset;
        public int Size;

        // Resources
        public int BindingSlot;

        /// <summary>
        /// Describes a value parameter.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public ParameterKeyInfo(ParameterKey key, int offset, int size)
        {
            Key = key;
            Offset = offset;
            Size = size;
            BindingSlot = -1;
        }

        /// <summary>
        /// Describes a resource parameter.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bindingSlot"></param>
        public ParameterKeyInfo(ParameterKey key, int bindingSlot)
        {
            Key = key;
            BindingSlot = bindingSlot;
            Offset = -1;
            Size = 1;
        }

        public override string ToString()
        {
            return $"{Key} ({(BindingSlot != -1 ? "BindingSlot " + BindingSlot : "Offset " + Offset)}, Size {Size})";
        }

        public bool Equals(ParameterKeyInfo other)
        {
            return Key.Equals(other.Key) && Offset == other.Offset && Size == other.Size && BindingSlot == other.BindingSlot;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ParameterKeyInfo && Equals((ParameterKeyInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Key.GetHashCode();
                hashCode = (hashCode*397) ^ Offset;
                hashCode = (hashCode*397) ^ Size;
                hashCode = (hashCode*397) ^ BindingSlot;
                return hashCode;
            }
        }

        public static bool operator ==(ParameterKeyInfo left, ParameterKeyInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ParameterKeyInfo left, ParameterKeyInfo right)
        {
            return !left.Equals(right);
        }
    }
}