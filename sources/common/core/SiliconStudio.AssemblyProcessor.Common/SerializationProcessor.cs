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
    public class SerializationProcessor : IAssemblyDefinitionProcessor
    {
        public string SignKeyFile { get; private set; }

        public List<string> References { get; private set; }

        public List<AssemblyDefinition> MemoryReferences { get; private set; }

        public ILogger Log { get; private set; }

        public SerializationProcessor(string signKeyFile, List<string> references, List<AssemblyDefinition> memoryReferences, ILogger log)
        {
            SignKeyFile = signKeyFile;
            References = references;
            MemoryReferences = memoryReferences;
            Log = log;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            // Generate serialization assembly
            var serializationAssemblyFilepath = ComplexSerializerGenerator.GenerateSerializationAssemblyLocation(context.Assembly.MainModule.FullyQualifiedName);
            context.Assembly = ComplexSerializerGenerator.GenerateSerializationAssembly(context.Platform, context.AssemblyResolver, context.Assembly, serializationAssemblyFilepath, SignKeyFile, References, MemoryReferences, Log);

            return true;
        }
    }
}