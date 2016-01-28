// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    internal class RenameAssemblyProcessor : IAssemblyDefinitionProcessor
    {
        private string assemblyName;

        public RenameAssemblyProcessor(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            context.Assembly.Name.Name = assemblyName;
            context.Assembly.MainModule.Name = assemblyName + ".dll";

            return true;
        }
    }
}