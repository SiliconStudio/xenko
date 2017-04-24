// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Engine.Network;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    // TODO: Make that private as soon as we stop signing assemblies (so that EffectCompilerServer can use it)
    public class RemoteEffectCompilerEffectAnswer : SocketMessage
    {
        // TODO: Support LoggerResult as well
        public EffectBytecode EffectBytecode { get; set; }

        public List<SerializableLogMessage> LogMessages { get; set; }

        public bool LogHasErrors { get; set; }
    }
}
