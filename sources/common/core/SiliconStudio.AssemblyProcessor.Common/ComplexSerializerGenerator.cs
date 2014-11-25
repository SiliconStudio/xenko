// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.Core;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Various helper functions to generate complex serializer.
    /// </summary>
    public static class ComplexSerializerGenerator
    {
        public static CodeDomProvider codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider();

        public static AssemblyDefinition GenerateSerializationAssembly(PlatformType platformType, BaseAssemblyResolver assemblyResolver, AssemblyDefinition assembly, string serializationAssemblyLocation, string signKeyFile = null)
        {
            // Create the serializer code generator
            var serializerGenerator = new ComplexSerializerCodeGenerator(assemblyResolver, assembly);

            // Register default serialization profile (to help AOT generic instantiation of serializers)
            RegisterDefaultSerializationProfile(assemblyResolver, assembly, serializerGenerator);

            // Generate serializer code
            var serializerGeneratedCode = serializerGenerator.TransformText();

            var compilerParameters = new CompilerParameters();
            compilerParameters.IncludeDebugInformation = false;
            compilerParameters.GenerateInMemory = false;
            compilerParameters.CompilerOptions = "/nostdlib";
            compilerParameters.TreatWarningsAsErrors = false;
            compilerParameters.TempFiles.KeepFiles = false;
            //Console.WriteLine("Generating {0}", serializationAssemblyLocation);

            // Add reference from source assembly
            // Use a hash set because it seems including twice mscorlib (2.0 and 4.0) seems to be a problem.
            var skipWindows = "Windows, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null";

            // Sign the serialization assembly the same way the source was signed
            // TODO: Transmit over command line
            if (assembly.Name.HasPublicKey)
            {
                // TODO: If delay signed, we could actually extract the public key and apply it ourself maybe?
                if (signKeyFile == null)
                    throw new InvalidOperationException("Generating serialization code for signed assembly, but no key was specified.");

                var assemblyPath = Path.GetDirectoryName(assembly.MainModule.FullyQualifiedName);
                if ((assembly.MainModule.Attributes & ModuleAttributes.StrongNameSigned) == ModuleAttributes.StrongNameSigned)
                {
                    // Strongly signed
                    compilerParameters.CompilerOptions += string.Format(" /keyfile:\"{0}\"", signKeyFile);
                }
                else
                {
                    // Delay signed
                    compilerParameters.CompilerOptions += string.Format(" /delaysign+ /keyfile:\"{0}\"", signKeyFile);
                }
            }

            var assemblyLocations = new HashSet<string>();
            foreach (var referencedAssemblyName in assembly.MainModule.AssemblyReferences)
            {
                if (referencedAssemblyName.FullName != skipWindows)
                {
                    if (assemblyLocations.Add(referencedAssemblyName.Name))
                    {
                        //Console.WriteLine("Resolve Assembly for serialization [{0}]", referencedAssemblyName.FullName);
                        compilerParameters.ReferencedAssemblies.Add(assemblyResolver.Resolve(referencedAssemblyName).MainModule.FullyQualifiedName);
                    }
                }
            }

            // typeof(Dictionary<,>)
            // Special case for 4.5: Because Dictionary<,> is forwarded, we need to add a reference to the actual assembly
            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            compilerParameters.ReferencedAssemblies.Add(mscorlibAssembly.MainModule.FullyQualifiedName);
            var collectionType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(Dictionary<,>).FullName);
            compilerParameters.ReferencedAssemblies.Add(collectionType.Module.FullyQualifiedName);

            // Make sure System and System.Reflection are added
            // TODO: Maybe we should do that for .NETCore and PCL too? (instead of WinRT only)
            if (platformType == PlatformType.WindowsStore || platformType == PlatformType.WindowsPhone)
            {
                if (assemblyLocations.Add("System"))
                {
                    compilerParameters.ReferencedAssemblies.Add(assemblyResolver.Resolve("System").MainModule.FullyQualifiedName);
                }
                if (assemblyLocations.Add("System.Reflection"))
                {
                    compilerParameters.ReferencedAssemblies.Add(assemblyResolver.Resolve("System.Reflection").MainModule.FullyQualifiedName);
                }
            }

            compilerParameters.ReferencedAssemblies.Add(assembly.MainModule.FullyQualifiedName);
            assemblyLocations.Add(assembly.Name.Name);

            // In case Paradox.Framework.Serialization was not referenced, let's add it.
            if (!assemblyLocations.Contains("SiliconStudio.Core"))
            {
                compilerParameters.ReferencedAssemblies.Add(assemblyResolver.Resolve("SiliconStudio.Core").MainModule.FullyQualifiedName);
            }

            compilerParameters.OutputAssembly = serializationAssemblyLocation;
            var compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, serializerGeneratedCode);

            if (compilerResults.Errors.HasErrors)
            {
                var errors = new StringBuilder();
                errors.AppendLine(string.Format("Serialization assembly compilation: {0} error(s)", compilerResults.Errors.Count));
                foreach (var error in compilerResults.Errors)
                    errors.AppendLine(error.ToString());
                throw new InvalidOperationException(errors.ToString());
            }

            // Run ILMerge
            var merge = new ILRepacking.ILRepack()
            {
                OutputFile = assembly.MainModule.FullyQualifiedName,
                DebugInfo = true,
                CopyAttributes = true,
                AllowMultipleAssemblyLevelAttributes = true,
                XmlDocumentation = false,
                NoRepackRes = true,
                PrimaryAssemblyDefinition = assembly,
                WriteToDisk = false,
                //KeepFirstOfMultipleAssemblyLevelAttributes = true,
                //Log = true,
                //LogFile = "ilmerge.log",
            };
            merge.SetInputAssemblies(new string[] { serializationAssemblyLocation });

            // Force to use the correct framework
            //merge.SetTargetPlatform("v4", frameworkFolder);
            merge.SetSearchDirectories(assemblyResolver.GetSearchDirectories());
            merge.Merge();

            // Add assembly signing info
            if (assembly.Name.HasPublicKey)
            {
                merge.TargetAssemblyDefinition.Name.PublicKey = assembly.Name.PublicKey;
                merge.TargetAssemblyDefinition.Name.PublicKeyToken = assembly.Name.PublicKeyToken;
                merge.TargetAssemblyDefinition.Name.Attributes |= AssemblyAttributes.PublicKey;
                if ((assembly.MainModule.Attributes & ModuleAttributes.StrongNameSigned) == ModuleAttributes.StrongNameSigned)
                    merge.TargetAssemblyMainModule.Attributes |= ModuleAttributes.StrongNameSigned;
            }

            try
            {
                // Delete serializer dll
                File.Delete(serializationAssemblyLocation);

                var serializationAssemblyPdbFilePath = Path.ChangeExtension(serializationAssemblyLocation, "pdb");
                if (File.Exists(serializationAssemblyPdbFilePath))
                {
                    File.Delete(serializationAssemblyPdbFilePath);
                }
            }
            catch (IOException)
            {
                // Mute IOException
            }

            return merge.TargetAssemblyDefinition;
        }

        private static void RegisterDefaultSerializationProfile(IAssemblyResolver assemblyResolver, AssemblyDefinition assembly, ComplexSerializerCodeGenerator generator)
        {
            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            if (mscorlibAssembly == null)
            {
                Console.WriteLine("Missing mscorlib.dll from assembly {0}", assembly.FullName);
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

        public static string GenerateSerializationAssemblyLocation(string assemblyLocation)
        {
            if (!Regex.IsMatch(assemblyLocation, ".dll|.exe", RegexOptions.IgnoreCase))
                throw new InvalidOperationException();

            return Regex.Replace(assemblyLocation, ".dll|.exe", ".Serializers.dll", RegexOptions.IgnoreCase);
        }

        private static bool CheckSerializationAssembly(string assemblyLocation, string serializationAssemblyLocation)
        {
            return (File.Exists(serializationAssemblyLocation)
                    && File.GetLastWriteTimeUtc(serializationAssemblyLocation) >= File.GetLastWriteTimeUtc(assemblyLocation));
        }
    }
}