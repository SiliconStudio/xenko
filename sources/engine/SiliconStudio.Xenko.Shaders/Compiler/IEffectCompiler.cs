// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Main interface used to compile a shader.
    /// </summary>
    public interface IEffectCompiler : IDisposable
    {
        /// <summary>
        /// Compiles the specified shader source.
        /// </summary>
        /// <param name="shaderSource">The shader source.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <returns>Result of the compilation.</returns>
        CompilerResults Compile(ShaderSource shaderSource, CompilerParameters compilerParameters);
    }
}
