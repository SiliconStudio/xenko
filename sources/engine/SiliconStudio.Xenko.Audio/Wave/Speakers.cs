// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Audio.Wave
{
    internal enum Speakers
    {
        All = -2147483648,
        BackCenter = 0x100,
        BackLeft = 0x10,
        BackRight = 0x20,
        FivePointOne = 0x3f,
        FivePointOneSurround = 0x60f,
        FourPointOne = 0x3b,
        FrontCenter = 4,
        FrontLeft = 1,
        FrontLeftOfCenter = 0x40,
        FrontRight = 2,
        FrontRightOfCenter = 0x80,
        LowFrequency = 8,
        Mono = 4,
        None = 0,
        Quad = 0x33,
        Reserved = 0x7ffc0000,
        SevenPointOne = 0xff,
        SevenPointOneSurround = 0x63f,
        SideLeft = 0x200,
        SideRight = 0x400,
        Stereo = 3,
        Surround = 0x107,
        TopBackCenter = 0x10000,
        TopBackLeft = 0x8000,
        TopBackRight = 0x20000,
        TopCenter = 0x800,
        TopFrontCenter = 0x2000,
        TopFrontLeft = 0x1000,
        TopFrontRight = 0x4000,
        TwoPointOne = 11
    }
}
