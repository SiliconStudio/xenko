// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    internal abstract partial class InputManagerWindows<TK> : InputManager<TK>
    {
        protected InputManagerWindows(IServiceRegistry registry) : base(registry)
        {
        }
    }
}
