// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A registry that builds itself based on assembly customs attributes
    /// </summary>
    /// <typeparam name="T">The type of the attribute that specifies the compiler to use</typeparam>
    /// <typeparam name="I">The type of the class implementing the <see cref="IAssetCompiler"/> interface to register</typeparam>
    public abstract class AttributeBasedRegistry<T, I> : CompilerRegistry<I> where T: CompilerAttribute where I: class, IAssetCompiler
    {
        private readonly Logger log = GlobalLogger.GetLogger("AssetsCompiler.AttributeBasedRegistry");

        private readonly HashSet<Assembly> registeredAssemblies = new HashSet<Assembly>();

        /// <summary>
        /// Create an instance of that registry
        /// </summary>
        protected AttributeBasedRegistry()
        {
            // Statically find all assemblies related to assets and register them
            var assemblies = AssemblyRegistry.Find(AssemblyCommonCategories.Assets);
            foreach (var assembly in assemblies)
                AnalyseAssembly(assembly);

            AssemblyRegistry.AssemblyRegistered += AssemblyRegistered;
        }

        /// <summary>
        /// Analyses an assembly and extracted asset compilers.
        /// </summary>
        /// <param name="assembly"></param>
        private void AnalyseAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            if (registeredAssemblies.Contains(assembly))
                return;
            
            // Process Asset types.
            foreach (var type in assembly.GetTypes())
            {
                // Only process Asset types
                if (!typeof(Asset).IsAssignableFrom(type) || !type.IsClass)
                    continue; 

                // Asset compiler
                var compilerAttribute = type.GetCustomAttribute<T>();

                if (compilerAttribute == null) // no compiler attribute in this asset
                    continue;

                try
                {
                    ProcessAttribute(compilerAttribute, type);
                }
                catch (Exception ex)
                {
                    log.Error("Unable to instantiate compiler [{0}]", ex, compilerAttribute.TypeName);
                }
            }
            registeredAssemblies.Add(assembly);
        }

        protected virtual bool ProcessAttribute(T compilerAttribute, Type type)
        {
            var compilerType = Type.GetType(compilerAttribute.TypeName);
            if (compilerType == null)
            {
                log.Error("Unable to find compiler [{0}] for asset [{1}]", compilerAttribute.TypeName, type);
                return false;
            }

            var compilerInstance = Activator.CreateInstance(compilerType) as I;
            if (compilerInstance == null)
            {
                log.Error("Invalid compiler type [{0}], must inherit from IAssetCompiler", compilerAttribute.TypeName);
                return false;
            }

            RegisterCompiler(type, compilerInstance);
            return true;
        }

        private void AssemblyRegistered(object sender, AssemblyRegisteredEventArgs e)
        {
            // Handle delay-loading assemblies
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
                AnalyseAssembly(e.Assembly);
        }
    }
}