// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    public class NullEffectCompiler : EffectCompilerBase
    {
        public override ObjectId GetShaderSourceHash(string type)
        {
            var url = GetStoragePathFromShaderType(type);
            ObjectId shaderSourceId;
            AssetManager.FileProvider.AssetIndexMap.TryGetValue(url, out shaderSourceId);
            return shaderSourceId;
        }

        public override EffectBytecode Compile(ShaderMixinSourceTree mixinTree, CompilerParameters compilerParameters, LoggerResult log)
        {
            throw new NotSupportedException("Shader Compilation is not allowed at run time on this platform.");
        }
    }
}