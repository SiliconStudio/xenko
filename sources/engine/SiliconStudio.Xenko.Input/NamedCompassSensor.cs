// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    public class NamedCompassSensor : NamedSensor, ICompassSensor
    {
        public float Heading => HeadingInternal;
        internal float HeadingInternal;

        public NamedCompassSensor(string systemName) : base(systemName, "Compass")
        {
        }
    }
}