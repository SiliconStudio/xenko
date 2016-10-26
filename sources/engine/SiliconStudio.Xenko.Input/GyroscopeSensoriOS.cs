// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GyroscopeSensoriOS : SensoriOS, IGyroscopeSensor
    {
        public Vector3 RotationRate => RotationRateInternal;
        internal Vector3 RotationRateInternal;
        
        public GyroscopeSensoriOS() : base("Gyroscope")
        {
        }
    }
}
#endif