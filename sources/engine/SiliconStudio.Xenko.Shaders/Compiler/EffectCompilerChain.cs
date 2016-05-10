// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Shaders.Compiler
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

        public override IVirtualFileProvider FileProvider
        {
            get { return compiler.FileProvider; }
            set { compiler.FileProvider = value; }
        }

        public override ObjectId GetShaderSourceHash(string type)
        {
            return compiler.GetShaderSourceHash(type);
        }

        public override void ResetCache(HashSet<string> modifiedShaders)
        {
            compiler.ResetCache(modifiedShaders);
        }

        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, EffectCompilerParameters effectParameters, CompilerParameters compilerParameters = null)
        {
            return compiler.Compile(mixinTree, effectParameters, compilerParameters);
        }
    }
}