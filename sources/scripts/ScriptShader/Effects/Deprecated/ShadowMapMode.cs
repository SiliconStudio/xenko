// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Rendering
{
    [Flags]
    public enum ShadowMapMode
    {
        None = 0,
        Caster = 1,
        Receiver = 2,
        All = Caster | Receiver,
    }
}
