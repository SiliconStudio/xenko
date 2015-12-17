using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public struct AttributeDescription
    {
        private readonly int hashCode;
        public override int GetHashCode() => hashCode;

        public AttributeDescription(string name)
        {
            hashCode = name.GetHashCode();
        }
    }
}
