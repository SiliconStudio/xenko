// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
namespace SiliconStudio.Xenko.Input
{
    public class CompassSensoriOS : SensoriOS, ICompassSensor
    {
        public float Heading => HeadingInternal;
        internal float HeadingInternal;

        public CompassSensoriOS() : base("Compass")
        {
        }

    }
}
#endif