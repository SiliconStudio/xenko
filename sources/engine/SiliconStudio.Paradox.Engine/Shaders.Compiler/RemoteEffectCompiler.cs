// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Shaders.Compiler.Internals;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    class RemoteEffectCompiler : EffectCompilerBase
    {
        private ShaderCompilerTarget shaderCompilerTarget;

        public override IVirtualFileProvider FileProvider
        {
            get { return null; }
            set {}
        }

        public RemoteEffectCompiler(ShaderCompilerTarget shaderCompilerTarget)
        {
            this.shaderCompilerTarget = shaderCompilerTarget;
        }

        public override ObjectId GetShaderSourceHash(string type)
        {
            var url = GetStoragePathFromShaderType(type);
            ObjectId shaderSourceId;
            AssetManager.FileProvider.AssetIndexMap.TryGetValue(url, out shaderSourceId);
            return shaderSourceId;
        }

        public override TaskOrResult<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            return CompileAsync(mixinTree, compilerParameters);
        }

        private async Task<EffectBytecodeCompilerResult> CompileAsync(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            return await shaderCompilerTarget.Compile(mixinTree, compilerParameters);
        }
    }

    public class RemoteEffectCompilerEffectRequest : SocketMessage
    {
        public ShaderMixinSource MixinTree { get; set; }
        
        // MixinTree.UsedParameters is DataMemberIgnore, so transmit it manually
        public ShaderMixinParameters UsedParameters { get; set; }
    }

    public class RemoteEffectCompilerEffectAnswer : SocketMessage
    {
        // TODO: Support LoggerResult as well
        public EffectBytecode EffectBytecode { get; set; }
    }

    [DataContract]
    public class RemoteEffectCompilerEffectRequested
    {
        public EffectCompileRequest Request { get; set; }
    }

    class ShaderCompilerTarget
    {
        private readonly object lockObject = new object();
        private readonly Guid? packageId;
        private Task<SocketMessageLayer> socketMessageLayerTask;

        public ShaderCompilerTarget(Guid? packageId)
        {
            this.packageId = packageId;
        }

        public void NotifyEffectRequested(EffectCompileRequest effectCompileRequest)
        {
            Task.Run(async () =>
            {
                // Send any effect request remotely (should fail if not connected)
                var socketMessageLayer = await GetOrCreateConnection();
                await socketMessageLayer.Send(new RemoteEffectCompilerEffectRequested { Request = effectCompileRequest });
            });
        }

        public async Task<SocketMessageLayer> Connect(Guid? packageId)
        {
            var url = string.Format("/service/{0}/SiliconStudio.Paradox.EffectCompilerServer.exe", ParadoxVersion.CurrentAsText);
            if (packageId.HasValue)
                url += string.Format("?packageid={0}", packageId.Value);

            var socketContext = await RouterClient.RequestServer(url);

            var socketMessageLayer = new SocketMessageLayer(socketContext, false);

            // Register network VFS
            NetworkVirtualFileProvider.RegisterServer(socketMessageLayer);

            Task.Run(() => socketMessageLayer.MessageLoop());

            return socketMessageLayer;
        }

        public async Task<EffectBytecodeCompilerResult> Compile(ShaderMixinSource mixinTree, CompilerParameters compilerParameters)
        {
            // Make sure we are connected
            // TODO: Handle reconnections, etc...
            var socketMessageLayer = await GetOrCreateConnection();

            var shaderCompilerAnswer = (RemoteEffectCompilerEffectAnswer)await socketMessageLayer.SendReceiveAsync(new RemoteEffectCompilerEffectRequest
            {
                MixinTree = mixinTree,
                UsedParameters = mixinTree.UsedParameters,
            });

            // TODO: Get LoggerResult as well
            return new EffectBytecodeCompilerResult(shaderCompilerAnswer.EffectBytecode);
        }

        private async Task<SocketMessageLayer> GetOrCreateConnection()
        {
            // Lazily connect
            lock (lockObject)
            {
                if (socketMessageLayerTask == null)
                    socketMessageLayerTask = Task.Run(() => Connect(packageId));
            }

            return await socketMessageLayerTask;
        }
    }
}