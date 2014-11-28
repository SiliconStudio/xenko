// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    public class NullEffectCompiler : EffectCompilerBase
    {
        public override EffectBytecode Compile(InternalCompilerParameters internalCompilerParameters)
        {
            throw new NotSupportedException("Shader Compilation is not allowed at run time on this platform.");
        }
    }
}