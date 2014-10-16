// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    class CustomAssemblyResolver : DefaultAssemblyResolver
    {
        /// <summary>
        /// Registers the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        public void Register(AssemblyDefinition assembly)
        {
            this.RegisterAssembly(assembly);
        }
    }
}