// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Particles
{
    public static class ParticleUtilities
    {
        public static int AlignedSize(int size, int alignment)
        {
            return (size % alignment == 0) ? size : (size + alignment - (size % alignment));
        }
    }
}

