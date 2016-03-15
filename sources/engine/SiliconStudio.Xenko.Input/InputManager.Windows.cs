// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
