// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_EFFECT_COMPILER
using System;

namespace SiliconStudio.Xenko.Shaders.Parser.Analysis
{
    [Flags]
    internal enum AssignmentOperatorStatus
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
    }
}
#endif