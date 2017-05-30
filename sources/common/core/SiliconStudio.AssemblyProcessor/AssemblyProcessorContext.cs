// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.IO;
using Mono.Cecil;
using SiliconStudio.Core;

namespace SiliconStudio.AssemblyProcessor
{
    internal class AssemblyProcessorContext
    {
        public CustomAssemblyResolver AssemblyResolver { get; private set; }
        public AssemblyDefinition Assembly { get; set; }
        public PlatformType Platform { get; private set; }
        public TextWriter Log { get; private set; }

        public AssemblyProcessorContext(CustomAssemblyResolver assemblyResolver, AssemblyDefinition assembly, PlatformType platform, TextWriter log)
        {
            AssemblyResolver = assemblyResolver;
            Assembly = assembly;
            Platform = platform;
            Log = log;
        }
    }
}
