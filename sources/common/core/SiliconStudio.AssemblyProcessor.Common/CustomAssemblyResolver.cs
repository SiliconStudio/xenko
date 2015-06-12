// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
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
        
        private HashSet<string> existingWindowsKitsReferenceAssemblies;

        /// <summary>
        /// Gets or sets the windows kits directory for Windows 10 apps.
        /// </summary>
        public string WindowsKitsReferenceDirectory { get; set; }

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

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (WindowsKitsReferenceDirectory != null)
            {
                if (existingWindowsKitsReferenceAssemblies == null)
                {
                    // First time, make list of existing assemblies in windows kits directory
                    existingWindowsKitsReferenceAssemblies = new HashSet<string>();

                    try
                    {
                        foreach (var directory in Directory.EnumerateDirectories(WindowsKitsReferenceDirectory))
                        {
                            existingWindowsKitsReferenceAssemblies.Add(Path.GetFileName(directory));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                // Look for this assembly in the windows kits directory
                if (existingWindowsKitsReferenceAssemblies.Contains(name.Name))
                {
                    var assemblyFile = Path.Combine(WindowsKitsReferenceDirectory, name.Name, name.Version.ToString(), name.Name + ".winmd");
                    if (File.Exists(assemblyFile))
                    {
                        if (parameters.AssemblyResolver == null)
                            parameters.AssemblyResolver = this;

                        return ModuleDefinition.ReadModule(assemblyFile, parameters).Assembly;
                    }
                }
            }

            return base.Resolve(name, parameters);
        }
    }
}