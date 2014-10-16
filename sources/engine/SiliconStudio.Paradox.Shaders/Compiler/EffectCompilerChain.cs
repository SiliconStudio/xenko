// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;

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

        public override EffectBytecode Compile(ShaderMixinSource mixin, string fullEffectName, ShaderMixinParameters compilerParameters, HashSet<string> modifiedShaders, HashSet<string> recentlyModifiedShaders, LoggerResult log)
        {
            return compiler.Compile(mixin, fullEffectName, compilerParameters, modifiedShaders, recentlyModifiedShaders, log);
        }
    }
}