// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    // TODO: improve names
    // TODO: allow the user to create its own layer
    [Flags]
    [DataContract]
    public enum RenderLayers
    {
        RenderLayerNone = 0,
        RenderLayer1 = 0x1,
        RenderLayer2 = 0x2,
        RenderLayer3 = 0x4,
        RenderLayer4 = 0x8,
        RenderLayer5 = 0x10,
        RenderLayer6 = 0x20,
        RenderLayer7 = 0x40,
        RenderLayer8 = 0x80,
        RenderLayer9 = 0x100,
        RenderLayer10 = 0x200,
        RenderLayer11 = 0x400,
        RenderLayer12 = 0x800,
        RenderLayer13 = 0x1000,
        RenderLayer14 = 0x2000,
        RenderLayer15 = 0x4000,

        RenderLayerAll = 0xffff
    }
}
