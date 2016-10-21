using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A registry that builds itself based on assembly types
    /// TODO: Maybe use and generalize AttributeBasedRegistry used in SiliconStudio.Assets.Compiler
    /// </summary>
    /// <typeparam name="TBase">The base class of the type to use</typeparam>
    public class TypeBasedRegistry<TBase> where TBase : class
    {
        private readonly Logger log = GlobalLogger.GetLogger("Input.TypeBasedRegistry");

        private readonly HashSet<Type> registeredTypes = new HashSet<Type>();
        private readonly HashSet<Assembly> registeredAssemblies = new HashSet<Assembly>();
        private readonly HashSet<Assembly> assembliesToRegister = new HashSet<Assembly>();
        private readonly Type baseType = typeof(TBase);

        private bool assembliesChanged;

        /// <summary>
        /// Create an instance of that registry
        /// </summary>
        public TypeBasedRegistry()
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

        public TBase CreateInstance(Type type)
        {
            AssertType(type);
            EnsureTypes();
            try
            {
                return (TBase)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                log.Error("Unable to instantiate type [{0}]", ex, type);
                return null;
            }
        }

        public IEnumerable<TBase> CreateAllInstances()
        {
            EnsureTypes();

            List<TBase> instances = new List<TBase>();
            foreach (var type in registeredTypes)
            {
                instances.Add(CreateInstance(type));
            }
            return instances;
        }

        private void RegisterTypesFromAssembly(Assembly assembly)
        {
            // Process Asset types.
            foreach (var type in assembly.GetTypes())
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);

                // Only process the correct types and make sure it is instantiatable
                if (!baseType.IsAssignableFrom(type) || type.IsAbstract || type.IsInterface || !type.IsClass || constructor == null)
                    continue;

                registeredTypes.Add(type);
            }
        }

        private void UnregisterTypesFromAssembly(Assembly assembly)
        {
            foreach (var typeToRemove in registeredTypes.Where(type => type.Assembly == assembly).ToList())
            {
                registeredTypes.Remove(typeToRemove);
            }
        }

        private
            void EnsureTypes
            ()
        {
            if (assembliesChanged)
            {
                foreach (var assembly in assembliesToRegister)
                {
                    if (!registeredAssemblies.Contains(assembly))
                    {
                        RegisterTypesFromAssembly(assembly);
                        registeredAssemblies.Add(assembly);
                    }
                }
                assembliesToRegister.Clear();
                assembliesChanged = false;
            }
        }

        private
            void AssertType
            (Type
                type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeof(TBase).IsAssignableFrom(type))
                throw new ArgumentException("Type [{0}] must be assignable to {1}".ToFormat(type, baseType), nameof(type));
        }

        private
            void RegisterAssembly
            (Assembly
                assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            assembliesToRegister.Add(assembly);
            assembliesChanged = true;
        }

        private
            void UnregisterAssembly
            (Assembly
                assembly)
        {
            registeredAssemblies.Remove(assembly);
            UnregisterTypesFromAssembly(assembly);
            assembliesChanged = true;
        }

        private
            void AssemblyRegistered
            (object sender, AssemblyRegisteredEventArgs e)
        {
            // Handle delay-loading assemblies
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
                RegisterAssembly(e.Assembly);
        }

        private
            void AssemblyUnregistered
            (object sender, AssemblyRegisteredEventArgs e)
        {
            if (e.Categories.Contains(AssemblyCommonCategories.Assets))
                UnregisterAssembly(e.Assembly);
        }
    }
}