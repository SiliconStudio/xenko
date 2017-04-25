// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    public class NullEffectCompiler : EffectCompilerBase
    {
        public override ObjectId GetShaderSourceHash(string type)
        {
            var url = GetStoragePathFromShaderType(type);
            ObjectId shaderSourceId;
            ContentManager.FileProvider.ContentIndexMap.TryGetValue(url, out shaderSourceId);
            return shaderSourceId;
        }

        public override IVirtualFileProvider FileProvider { get; set; }

        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, EffectCompilerParameters effectParameters, CompilerParameters compilerParameters = null)
        {
            throw new NotSupportedException("Shader Compilation is not allowed at run time on this platform.");
        }
    }
}
