// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles
{
    static public class ParticleUtilities
    {    
        public static int AlignedSize(int size, int alignment)
        {
            return (size % alignment == 0) ? size : (size + alignment - (size % alignment));
        }
    }
}

