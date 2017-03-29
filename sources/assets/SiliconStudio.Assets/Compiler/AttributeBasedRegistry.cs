// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Compiler
{
    public abstract class AttributeBasedRegistry<I> : CompilerRegistry<I> where I : class, IAssetCompiler
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
                if (!typeof(IAssetCompiler).IsAssignableFrom(type) || !type.IsClass)
                    continue;

                // Asset compiler
                var compilerAttribute = type.GetCustomAttribute<AssetCompilerAttribute>();

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

        protected virtual bool ProcessAttribute(AssetCompilerAttribute compilerCompilerAttribute, Type type)
        {
            if (!typeof(ICompilationContext).IsAssignableFrom(compilerCompilerAttribute.CompilationContext))
            {
                log.Error($"Invalid compiler context type [{compilerCompilerAttribute.CompilationContext}], must inherit from ICompilerContext");
                return false;
            }

            var assetType = AssemblyRegistry.GetType(compilerCompilerAttribute.TypeName);
            if (assetType == null)
            {
                log.Error($"Unable to find asset [{compilerCompilerAttribute.TypeName}] for compiler [{type}]");
                return false;
            }

            var compilerInstance = Activator.CreateInstance(type) as I;
            if (compilerInstance == null)
            {
                log.Error($"Invalid compiler type [{type}], must inherit from IAssetCompiler");
                return false;
            }

            RegisterCompiler(assetType, compilerInstance, compilerCompilerAttribute.CompilationContext);
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
