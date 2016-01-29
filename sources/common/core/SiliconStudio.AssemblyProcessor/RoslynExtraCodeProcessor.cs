// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    internal class RoslynExtraCodeProcessor : IAssemblyDefinitionProcessor
    {
        public string SignKeyFile { get; private set; }

        public List<string> References { get; private set; }

        public List<AssemblyDefinition> MemoryReferences { get; private set; }

        public TextWriter Log { get; private set; }

        public List<string> SourceCodes { get; } = new List<string>();

        public RoslynExtraCodeProcessor(string signKeyFile, List<string> references, List<AssemblyDefinition> memoryReferences, TextWriter log)
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