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
        private readonly HashSet<Assembly> assembliesToRegister = new HashSet<Assembly>();

        private bool assembliesChanged;

        /// <summary>
        /// Create an instance of that registry
        /// </summary>
        protected AttributeBasedRegistry()
        {
            // Statically find all assemblies related to assets and register them
            var assemblies = AssemblyRegistry.Find(AssemblyCommonCategories.Assets);
            foreach (var assembly in assemblies)
            {
                RegisterAssembly(assembly);
            }

            AssemblyRegistry.AssemblyRegistered += AssemblyRegistered;
            AssemblyRegistry.AssemblyUnregistered += AssemblyUnregistered;
        }

        private void RegisterCompilersFromAssembly(Assembly assembly)
        {
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
                    log.Error($"Unable to instantiate compiler [{compilerAttribute.TypeName}]", ex);
                }
            }
        }

        protected virtual bool ProcessAttribute(T compilerAttribute, Type type)
        {
            var compilerType = AssemblyRegistry.GetType(compilerAttribute.TypeName);
            if (compilerType == null)
            {
                log.Error($"Unable to find compiler [{compilerAttribute.TypeName}] for asset [{type}]");
                return false;
            }

            var compilerInstance = Activator.CreateInstance(compilerType) as I;
            if (compilerInstance == null)
            {
                log.Error($"Invalid compiler type [{compilerAttribute.TypeName}], must inherit from IAssetCompiler");
                return false;
            }

            RegisterCompiler(type, compilerInstance);
            return true;
        }

        protected override void EnsureTypes()
        {
            if (assembliesChanged)
            {
                foreach (var assembly in assembliesToRegister)
                {
                    if (!registeredAssemblies.Contains(assembly))
                    {
                        RegisterCompilersFromAssembly(assembly);
                        registeredAssemblies.Add(assembly);
                    }
                }
                assembliesToRegister.Clear();
                assembliesChanged = false;
            }
        }

        private void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            assembliesToRegister.Add(assembly);
            assembliesChanged = true;
        }

        private void UnregisterAssembly(Assembly assembly)
        {
            registeredAssemblies.Remove(assembly);
            UnregisterCompilersFromAssembly(assembly);
            assembliesChanged = true;
        }

        private void AssemblyRegistered(object sender, AssemblyRegisteredEventArgs e)
        {
            // Handle delay-loading assemblies
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
                RegisterAssembly(e.Assembly);
        }

        private void AssemblyUnregistered(object sender, AssemblyRegisteredEventArgs e)
        {
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
                UnregisterAssembly(e.Assembly);
        }
    }
}
