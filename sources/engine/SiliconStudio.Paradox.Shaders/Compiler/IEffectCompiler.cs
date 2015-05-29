// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Main interface used to compile a shader.
    /// </summary>
    public interface IEffectCompiler
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