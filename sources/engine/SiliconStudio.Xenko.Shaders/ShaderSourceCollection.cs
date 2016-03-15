using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
{
    [DataContract]
    public sealed class ShaderSourceCollection : List<ShaderSource>
    {
        public ShaderSourceCollection()
        {
        }

        public ShaderSourceCollection(IEnumerable<ShaderSource> collection) : base(collection)
        {
        }

        public override int GetHashCode()
        {
            return Utilities.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ShaderSourceCollection)obj);
        }

        public bool Equals(ShaderSourceCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; ++i)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }
    }
}