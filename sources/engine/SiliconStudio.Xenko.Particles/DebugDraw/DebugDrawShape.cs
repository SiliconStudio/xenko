using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Particles.DebugDraw
{
    public enum DebugDrawShape
    {
        None        = 0,
        Sphere      = 1, // Spere centered at the origin O with a default radius of 1
        Cube        = 2, // Cube centered at the origin O with a side of 2, each corner at (-1,-1,-1) to (+1,+1,+1)
        Cone        = 3, // Cone, starting at the origin O, spreading out to (0, +1, 0) with a radius of 1
    }
}
