// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Allow to register assemblies manually, with their in-memory representation if necessary.
    /// </summary>
    public class CustomAssemblyResolver : DefaultAssemblyResolver
    {
        /// <summary>
        /// Assemblies stored as byte arrays.
        /// </summary>
        private readonly Dictionary<AssemblyDefinition, byte[]> assemblyData = new Dictionary<AssemblyDefinition, byte[]>();

        /// <summary>
        /// Registers the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        public void Register(AssemblyDefinition assembly)
        {
            this.RegisterAssembly(assembly);
        }

        /// <summary>
        /// Gets the assembly data (if it exists).
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public byte[] GetAssemblyData(AssemblyDefinition assembly)
        {
            byte[] data;
            assemblyData.TryGetValue(assembly, out data);
            return data;
        }

        /// <summary>
        /// Registers the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        public void Register(AssemblyDefinition assembly, byte[] peData)
        {
            assemblyData[assembly] = peData;
            this.RegisterAssembly(assembly);
        }
    }
}