// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Engine.Network;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    // TODO: Make that private as soon as we stop signing assemblies (so that EffectCompilerServer can use it)
    public class RemoteEffectCompilerEffectRequest : SocketMessage
    {
        public ShaderMixinSource MixinTree { get; set; }
        
        public EffectCompilerParameters EffectParameters { get; set; }
    }
}