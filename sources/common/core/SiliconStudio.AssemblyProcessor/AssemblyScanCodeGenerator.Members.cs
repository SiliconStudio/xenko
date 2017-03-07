using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    public partial class AssemblyScanCodeGenerator
    {
        private readonly string assemblyScanClassName;
        private readonly Dictionary<TypeReference, HashSet<TypeDefinition>> scanTypes = new Dictionary<TypeReference, HashSet<TypeDefinition>>(TypeReferenceEqualityComparer.Default);

        public AssemblyScanCodeGenerator(AssemblyDefinition assembly)
        {
            this.assemblyScanClassName = Utilities.BuildValidClassName(assembly.Name.Name) + "AssemblyScan";
        }

        public bool HasScanTypes => scanTypes.Count > 0;

        public void Register(TypeDefinition type, TypeReference scanType)
        {
            HashSet<TypeDefinition> types;
            if (!scanTypes.TryGetValue(scanType, out types))
            {
                types = new HashSet<TypeDefinition>();
                scanTypes.Add(scanType, types);
            }

            types.Add(type);
        }
    }
}