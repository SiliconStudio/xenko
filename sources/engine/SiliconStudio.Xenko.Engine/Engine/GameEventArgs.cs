// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Engine
{
    public class GameEventArgs : EventArgs
    {
        public Game Game { get; internal set; }
    }
}