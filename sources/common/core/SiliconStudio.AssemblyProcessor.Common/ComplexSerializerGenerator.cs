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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.Core;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Various helper functions to compile low-level "complex" serializer.
    /// </summary>
    public static class ComplexSerializerGenerator
    {
        public static CodeDomProvider codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider();

        public static AssemblyDefinition GenerateSerializationAssembly(PlatformType platformType, BaseAssemblyResolver assemblyResolver, AssemblyDefinition assembly, string serializationAssemblyLocation, string signKeyFile, List<string> serializatonProjectReferencePaths)
        {
            // Make sure all assemblies in serializatonProjectReferencePaths are referenced (sometimes they might be optimized out if no direct references)
            foreach (var serializatonProjectReferencePath in serializatonProjectReferencePaths)
            {
                var shortAssemblyName = Path.GetFileNameWithoutExtension(serializatonProjectReferencePath);

                // Still in references (not optimized)
                if (assembly.MainModule.AssemblyReferences.Any(x => x.Name == shortAssemblyName))
                    continue;

                // For now, use AssemblyDefinition.ReadAssembly to compute full name -- maybe not very efficient but it shouldn't happen often anyway)
                var referencedAssembly = AssemblyDefinition.ReadAssembly(serializatonProjectReferencePath);

                assembly.MainModule.AssemblyReferences.Add(AssemblyNameReference.Parse(referencedAssembly.FullName));
            }

            // Create the serializer code generator
            var serializerGenerator = new ComplexSerializerCodeGenerator(assemblyResolver, assembly);

            // Register default serialization profile (to help AOT generic instantiation of serializers)
            RegisterDefaultSerializationProfile(assemblyResolver, assembly, serializerGenerator);

            // Generate serializer code
            var serializerGeneratedCode = serializerGenerator.TransformText();

            var syntaxTree = CSharpSyntaxTree.ParseText(serializerGeneratedCode);

            // Add reference from source assembly
            // Use a hash set because it seems including twice mscorlib (2.0 and 4.0) seems to be a problem.
            var skipWindows = "Windows, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null";

            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

            // Sign the serialization assembly the same way the source was signed
            // TODO: Transmit over command line
            if (assembly.Name.HasPublicKey)
            {
                // TODO: If delay signed, we could actually extract the public key and apply it ourself maybe?
                if (signKeyFile == null)
                    throw new InvalidOperationException("Generating serialization code for signed assembly, but no key was specified.");

                compilerOptions = compilerOptions.WithCryptoKeyFile(signKeyFile).WithStrongNameProvider(new DesktopStrongNameProvider());
                if ((assembly.MainModule.Attributes & ModuleAttributes.StrongNameSigned) != ModuleAttributes.StrongNameSigned)
                {
                    // Delay signed
                    compilerOptions = compilerOptions.WithDelaySign(true);
                }
            }

            var metadataReferences = new List<MetadataReference>();
            var assemblyLocations = new HashSet<string>();
            foreach (var referencedAssemblyName in assembly.MainModule.AssemblyReferences)
            {
                // We skip both Windows, and current assembly (AssemblyProcessor.Common, which might be added with an alias)
                if (referencedAssemblyName.FullName != skipWindows && referencedAssemblyName.FullName != typeof(ComplexSerializerGenerator).Assembly.FullName && referencedAssemblyName.FullName != "SiliconStudio.AssemblyProcessor")
                {
                    if (assemblyLocations.Add(referencedAssemblyName.Name))
                    {
                        //Console.WriteLine("Resolve Assembly for serialization [{0}]", referencedAssemblyName.FullName);
                        metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve(referencedAssemblyName)));
                    }
                }
            }

            // typeof(Dictionary<,>)
            // Special case for 4.5: Because Dictionary<,> is forwarded, we need to add a reference to the actual assembly
            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            metadataReferences.Add(CreateMetadataReference(assemblyResolver, mscorlibAssembly));
            var collectionType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(Dictionary<,>).FullName);
            metadataReferences.Add(CreateMetadataReference(assemblyResolver, collectionType.Module.Assembly));

            // Make sure System and System.Reflection are added
            // TODO: Maybe we should do that for .NETCore and PCL too? (instead of WinRT only)
            if (platformType == PlatformType.WindowsStore || platformType == PlatformType.WindowsPhone)
            {
                if (assemblyLocations.Add("System"))
                {
                    metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve("System")));
                }
                if (assemblyLocations.Add("System.Reflection"))
                {
                    metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve("System.Reflection")));
                }
            }

            metadataReferences.Add(CreateMetadataReference(assemblyResolver, assembly));
            assemblyLocations.Add(assembly.Name.Name);

            // In case Paradox.Framework.Serialization was not referenced, let's add it.
            if (!assemblyLocations.Contains("SiliconStudio.Core"))
            {
                metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve("SiliconStudio.Core")));
                assemblyLocations.Add("SiliconStudio.Core");
            }

            // Create roslyn compilation object
            var assemblyName = Path.GetFileNameWithoutExtension(serializationAssemblyLocation);
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, metadataReferences, compilerOptions);

            // Do the actual compilation, and check errors
            using (var peStream = new FileStream(serializationAssemblyLocation, FileMode.Create, FileAccess.Write))
            {
                var compilationResult = compilation.Emit(peStream);

                if (!compilationResult.Success)
                {
                    var errors = new StringBuilder();
                    errors.AppendLine(string.Format("Serialization assembly compilation: {0} error(s)", compilationResult.Diagnostics.Count(x => x.Severity >= DiagnosticSeverity.Error)));
                    foreach (var error in compilationResult.Diagnostics)
                    {
                        if (error.Severity >= DiagnosticSeverity.Warning)
                            errors.AppendLine(error.ToString());
                    }
                    throw new InvalidOperationException(errors.ToString());
                }
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

            // Copy name
            merge.TargetAssemblyDefinition.Name.Name = assembly.Name.Name;
            merge.TargetAssemblyDefinition.Name.Version = assembly.Name.Version;

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

        private static MetadataReference CreateMetadataReference(IAssemblyResolver assemblyResolver, AssemblyDefinition assembly)
        {
            // Try to find if it has been registed with a in-memory version first
            var customAssemblyResolver = assemblyResolver as CustomAssemblyResolver;
            if (customAssemblyResolver != null)
            {
                var assemblyData = customAssemblyResolver.GetAssemblyData(assembly);
                if (assemblyData != null)
                {
                    return MetadataReference.CreateFromStream(new MemoryStream(assemblyData));
                }
            }

            return MetadataReference.CreateFromFile(assembly.MainModule.FullyQualifiedName);
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