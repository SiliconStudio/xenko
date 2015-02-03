// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    [Flags]
    public enum LightParamSemantic
    {
        PositionVS = 0x1,
        DirectionVS = 0x2,
        PositionWS = 0x4,
        DirectionWS = 0x8,
        ColorWithGamma = 0x10,
        Intensity = 0x20,
        Decay = 0x40,
        SpotBeamAngle = 0x80,
        SpotFieldAngle = 0x100,
        Count = 0x200,

        PositionDirectionVS = PositionVS | DirectionVS,
        PositionDirectionWS = PositionWS | DirectionWS
    }
}