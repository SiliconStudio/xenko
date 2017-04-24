// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Compiles effect remotely on the developer host PC.
    /// </summary>
    class RemoteEffectCompiler : EffectCompilerBase
    {
        private RemoteEffectCompilerClient remoteEffectCompilerClient;

        /// <inheritdoc/>
        public override IVirtualFileProvider FileProvider
        {
            get { return null; }
            set {}
        }

        public RemoteEffectCompiler(RemoteEffectCompilerClient remoteEffectCompilerClient)
        {
            this.remoteEffectCompilerClient = remoteEffectCompilerClient;
        }

        /// <inheritdoc/>
        public override ObjectId GetShaderSourceHash(string type)
        {
            var url = GetStoragePathFromShaderType(type);
            ObjectId shaderSourceId;
            ContentManager.FileProvider.ContentIndexMap.TryGetValue(url, out shaderSourceId);
            return shaderSourceId;
        }

        /// <inheritdoc/>
        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, EffectCompilerParameters effectParameters, CompilerParameters compilerParameters = null)
        {
            return CompileAsync(mixinTree, effectParameters);
        }

        private async Task<EffectBytecodeCompilerResult> CompileAsync(ShaderMixinSource mixinTree, EffectCompilerParameters effectParameters)
        {
            return await remoteEffectCompilerClient.Compile(mixinTree, effectParameters);
        }
    }
}
