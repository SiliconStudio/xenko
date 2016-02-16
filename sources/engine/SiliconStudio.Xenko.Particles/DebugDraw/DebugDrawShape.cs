// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.DebugDraw
{
    public enum DebugDrawShape
    {
        None        = 0,
        Sphere      = 1, // Spere centered at the origin O with a default radius of 1
        Cube        = 2, // Cube centered at the origin O with a side of 2, each corner at (-1,-1,-1) to (+1,+1,+1)
        Cone        = 3, // Cone, starting at the origin O, spreading out to (0, +1, 0) with a radius of 1
        Cylinder    = 4, // Cylinder, starting at the origin O, with a height of 1 and a radius of 1
        Torus       = 5, // Torus, centered around O, with a big radius of 1 and a small radius of 0.5
    }
}
