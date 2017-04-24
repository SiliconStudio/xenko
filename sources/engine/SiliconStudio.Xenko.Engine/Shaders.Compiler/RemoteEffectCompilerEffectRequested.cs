// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    [DataContract]
    public class RemoteEffectCompilerEffectRequested
    {
        // EffectCompileRequest serialized (so that it can be forwarded by EffectCompilerServer without being deserialized, since it might contain unknown types)
        public byte[] Request { get; set; }
    }
}
