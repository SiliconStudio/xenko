// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Helper class that delegates actual compilation to another <see cref="IEffectCompiler"/>.
    /// </summary>
    public class EffectCompilerChain : EffectCompilerBase
    {
        private readonly EffectCompilerBase compiler;

        public EffectCompilerChain(EffectCompilerBase compiler)
        {
            if (compiler == null) throw new ArgumentNullException("compiler");
            this.compiler = compiler;
        }

        protected EffectCompilerBase Compiler
        {
            get { return compiler; }
        }

        public override EffectBytecode Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log)
        {
            return compiler.Compile(mixinTree, compilerParameters, log);
        }
    }
}