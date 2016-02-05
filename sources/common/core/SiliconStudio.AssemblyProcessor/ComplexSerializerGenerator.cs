// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Various helper functions to compile low-level "complex" serializer.
    /// </summary>
    public static class ComplexSerializerGenerator
    {
        public static CodeDomProvider codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider();

        public static string GenerateSerializationAssembly(BaseAssemblyResolver assemblyResolver, AssemblyDefinition assembly, TextWriter log)
        {
            // Create the serializer code generator
            var serializerGenerator = new ComplexSerializerCodeGenerator(assemblyResolver, assembly, log);

            // Register default serialization profile (to help AOT generic instantiation of serializers)
            RegisterDefaultSerializationProfile(assemblyResolver, assembly, serializerGenerator, log);

            // Generate serializer code
            return serializerGenerator.TransformText();
        }

        private static void RegisterDefaultSerializationProfile(IAssemblyResolver assemblyResolver, AssemblyDefinition assembly, ComplexSerializerCodeGenerator generator, TextWriter log)
        {
            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            if (mscorlibAssembly == null)
            {
                log.WriteLine("Missing mscorlib.dll from assembly {0}", assembly.FullName);
                throw new InvalidOperationException("Missing mscorlib.dll from assembly");
            }

            var coreSerializationAssembly = assemblyResolver.Resolve("SiliconStudio.Core");

            // Register serializer factories (determine which type requires which serializer)
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(IList<>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.ListInterfaceSerializer`1")));
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(List<>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.ListSerializer`1")));
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(KeyValuePair<,>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.KeyValuePairSerializer`2")));
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(IDictionary<,>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.DictionaryInterfaceSerializer`2")));
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(Dictionary<,>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.DictionarySerializer`2")));
            generator.SerializerFactories.Add(new CecilGenericSerializerFactory(typeof(Nullable<>), coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.NullableSerializer`1")));
            generator.SerializerFactories.Add(new CecilEnumSerializerFactory(coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.EnumSerializer`1")));
            generator.SerializerFactories.Add(new CecilArraySerializerFactory(coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.Serializers.ArraySerializer`1")));

            // Iterate over tuple size
            for (int i = 1; i <= 4; ++i)
            {
                generator.SerializerDependencies.Add(new CecilSerializerDependency(
                                                         string.Format("System.Tuple`{0}", i),
                                                         coreSerializationAssembly.MainModule.GetTypeResolved(string.Format("SiliconStudio.Core.Serialization.Serializers.TupleSerializer`{0}", i))));

                generator.SerializerDependencies.Add(new CecilSerializerDependency(string.Format("SiliconStudio.Core.Serialization.Serializers.TupleSerializer`{0}", i)));
            }

            // Register serializer dependencies (determine which serializer serializes which sub-type)
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.ArraySerializer`1"));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.KeyValuePairSerializer`2"));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.ListSerializer`1"));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.ListInterfaceSerializer`1"));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.NullableSerializer`1"));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.DictionarySerializer`2",
                                                                               mscorlibAssembly.MainModule.GetTypeResolved(typeof(KeyValuePair<,>).FullName)));
            generator.SerializerDependencies.Add(new CecilSerializerDependency("SiliconStudio.Core.Serialization.Serializers.DictionaryInterfaceSerializer`2",
                                                                               mscorlibAssembly.MainModule.GetTypeResolved(typeof(KeyValuePair<,>).FullName)));
        }
    }
}
