// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GravitySensoriOS : SensoriOS, IGravitySensor
    {
        public Vector3 Vector => VectorInternal;
        internal Vector3 VectorInternal;

        public GravitySensoriOS() : base("Gravity")
        {
        }

    }
}
#endif