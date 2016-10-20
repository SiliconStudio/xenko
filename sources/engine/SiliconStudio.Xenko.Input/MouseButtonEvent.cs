// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    public class MouseButtonEvent : EventArgs
    {
        public MouseButton Button;
        public MouseButtonState State;
    }
}