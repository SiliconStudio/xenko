// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using Mono.Cecil;
using SiliconStudio.Core;

namespace SiliconStudio.AssemblyProcessor
{
    internal class AssemblyProcessorContext
    {
        public BaseAssemblyResolver AssemblyResolver { get; private set; }
        public AssemblyDefinition Assembly { get; set; }
        public PlatformType Platform { get; private set; }
        public TextWriter Log { get; private set; }

        public AssemblyProcessorContext(BaseAssemblyResolver assemblyResolver, AssemblyDefinition assembly, PlatformType platform, TextWriter log)
        {
            AssemblyResolver = assemblyResolver;
            Assembly = assembly;
            Platform = platform;
            Log = log;
        }
    }
}