// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.AssemblyProcessor
{
    public class RoslynExtraCodeProcessor : IAssemblyDefinitionProcessor
    {
        public string SignKeyFile { get; private set; }

        public List<string> References { get; private set; }

        public List<AssemblyDefinition> MemoryReferences { get; private set; }

        public ILogger Log { get; private set; }

        public List<string> SourceCodes { get; } = new List<string>();

        public RoslynExtraCodeProcessor(string signKeyFile, List<string> references, List<AssemblyDefinition> memoryReferences, ILogger log)
        {
            SignKeyFile = signKeyFile;
            References = references;
            MemoryReferences = memoryReferences;
            Log = log;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            if (SourceCodes.Count == 0)
                return false;

            // Generate serialization assembly
            var serializationAssemblyFilepath = RoslynCodeMerger.GenerateRolsynAssemblyLocation(context.Assembly.MainModule.FullyQualifiedName);
            context.Assembly = RoslynCodeMerger.GenerateRoslynAssembly(context.AssemblyResolver, context.Assembly, serializationAssemblyFilepath, SignKeyFile, References, MemoryReferences, Log, SourceCodes);

            return true;
        }
    }
}