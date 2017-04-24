// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ILRepacking;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Various helper functions to compile low-level "complex" serializer.
    /// </summary>
    public static class RoslynCodeMerger
    {
        public static AssemblyDefinition GenerateRoslynAssembly(CustomAssemblyResolver assemblyResolver, AssemblyDefinition assembly, string serializationAssemblyLocation, string signKeyFile, List<string> references, List<AssemblyDefinition> memoryReferences, TextWriter log, IEnumerable<SourceCode> sourceCodes)
        {
            var sourceFolder = Path.GetDirectoryName(serializationAssemblyLocation);
            var syntaxTrees = sourceCodes.Select(x =>
            {
                // It has a name, let's save it as a file
                string sourcePath = null;
                if (x.Name != null)
                {
                    sourcePath = Path.Combine(sourceFolder, $"{x.Name}.cs");
                    File.WriteAllText(sourcePath, x.Code);
                }

                var result = CSharpSyntaxTree.ParseText(x.Code, null, sourcePath, Encoding.UTF8);

                return result;
            }).ToArray();

            var compilerOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

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

            // Add references (files and in-memory PE data)
            var metadataReferences = new List<MetadataReference>();
            foreach (var reference in assemblyResolver.References)
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(reference));
            }

            foreach (var reference in memoryReferences)
            {
                metadataReferences.Add(CreateMetadataReference(assemblyResolver, reference));
            }

            // typeof(Dictionary<,>)
            // Special case for 4.5: Because Dictionary<,> is forwarded, we need to add a reference to the actual assembly
            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            metadataReferences.Add(CreateMetadataReference(assemblyResolver, mscorlibAssembly));
            var collectionAssembly = CecilExtensions.FindCollectionsAssembly(assembly);
            metadataReferences.Add(CreateMetadataReference(assemblyResolver, collectionAssembly));

            // Open file currently being processed using FileShare.ReadWrite
            using (var stream = File.Open(assembly.MainModule.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                metadataReferences.Add(MetadataReference.CreateFromStream(stream, filePath: assembly.MainModule.FileName));

            // In case SiliconStudio.Core was not referenced, let's add it.
            if (assembly.Name.Name != "SiliconStudio.Core" && !references.Any(x => string.Compare(Path.GetFileNameWithoutExtension(x), "SiliconStudio.Core", StringComparison.OrdinalIgnoreCase) == 0))
            {
                metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve(new AssemblyNameReference("SiliconStudio.Core", null))));
            }

            // Create roslyn compilation object
            var assemblyName = assembly.Name.Name + ".Serializers";
            var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, metadataReferences, compilerOptions);

            // Do the actual compilation, and check errors
            using (var peStream = new FileStream(serializationAssemblyLocation, FileMode.Create, FileAccess.Write))
            using (var pdbStream = new FileStream(Path.ChangeExtension(serializationAssemblyLocation, ".pdb"), FileMode.Create, FileAccess.Write))
            {
                var compilationResult = compilation.Emit(peStream, pdbStream);

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

            // Make sure every instruction in the primary assembly has offset up to date
            // Ideally, we should do it manually only on method whose instructions changed so far
            foreach (var type in assembly.MainModule.Types)
            {
                GenerateOffsetForMethodsOfType(type);
            }

            var repackOptions = new ILRepacking.RepackOptions(new string[0])
            {
                OutputFile = assembly.MainModule.FileName,
                DebugInfo = true,
                CopyAttributes = true,
                AllowMultipleAssemblyLevelAttributes = true,
                XmlDocumentation = false,
                NoRepackRes = true,
                InputAssemblies = new[] { serializationAssemblyLocation },
                SearchDirectories = new string[0],
            };

            // Run ILMerge
            var merge = new ILRepacking.ILRepack(repackOptions)
            {
                GlobalAssemblyResolver = new RepackAssemblyResolverAdapter(assemblyResolver),
                PrimaryAssemblyDefinition = assembly,
                MemoryOnly = true,
                //KeepFirstOfMultipleAssemblyLevelAttributes = true,
                //Log = true,
                //LogFile = "ilmerge.log",
            };

            try
            {
                var consoleWriter = Console.Out;
                Console.SetOut(TextWriter.Null);

                try
                {
                    merge.Repack();
                }
                finally
                {
                    Console.SetOut(consoleWriter);
                }
            }
            catch (Exception)
            {
                log.WriteLine($"Error while ILRepacking {assembly.Name.Name}");
                throw;
            }

            // Copy name
            merge.TargetAssemblyDefinition.Name.Name = assembly.Name.Name;
            merge.TargetAssemblyDefinition.Name.Version = assembly.Name.Version;

            // Copy assembly characterics. This is necessary especially when targeting a windows app
            merge.TargetAssemblyMainModule.Characteristics = assembly.MainModule.Characteristics;

            // Add assembly signing info
            if (assembly.Name.HasPublicKey)
            {
                merge.TargetAssemblyDefinition.Name.PublicKey = assembly.Name.PublicKey;
                merge.TargetAssemblyDefinition.Name.PublicKeyToken = assembly.Name.PublicKeyToken;
                merge.TargetAssemblyDefinition.Name.Attributes |= AssemblyAttributes.PublicKey;
                if ((assembly.MainModule.Attributes & ModuleAttributes.StrongNameSigned) == ModuleAttributes.StrongNameSigned)
                    merge.TargetAssemblyMainModule.Attributes |= ModuleAttributes.StrongNameSigned;
            }

            // Dispose old assembly
            assembly.Dispose();

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

        /// <summary>
        /// Recompute offsets for method instructions.
        /// </summary>
        /// <param name="type"></param>
        private static void GenerateOffsetForMethodsOfType(TypeDefinition type)
        {
            foreach (var nestedType in type.NestedTypes)
            {
                GenerateOffsetForMethodsOfType(nestedType);
            }

            foreach (var method in type.Methods)
            {
                var body = method.Body;
                if (body == null)
                    continue;

                int offset = 0;
                var instructions = body.Instructions;
                for (int i = 0; i < instructions.Count; i++)
                {
                    var instruction = body.Instructions[i];
                    instruction.Offset = offset;
                    offset += instruction.GetSize();
                }
            }
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

            var filename = assembly.MainModule.FileName;
            return MetadataReference.CreateFromFile(filename);
        }

        public static string GenerateRolsynAssemblyLocation(string assemblyLocation)
        {
            if (!Regex.IsMatch(assemblyLocation, ".dll|.exe", RegexOptions.IgnoreCase))
                throw new InvalidOperationException();

            // For historic reason, it is named *.Serializers.dll
            return Regex.Replace(assemblyLocation, ".dll|.exe", ".Serializers.dll", RegexOptions.IgnoreCase);
        }

        public struct SourceCode
        {
            public string Code;
            public string Name;
        }

        class RepackAssemblyResolverAdapter : IRepackAssemblyResolver
        {
            private readonly CustomAssemblyResolver assemblyResolver;

            public RepackAssemblyResolverAdapter(CustomAssemblyResolver assemblyResolver)
            {
                this.assemblyResolver = assemblyResolver;
            }

            public void Dispose()
            {
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return assemblyResolver.Resolve(name);
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                return assemblyResolver.Resolve(name, parameters);
            }

            public void RegisterAssembly(AssemblyDefinition assembly)
            {
                assemblyResolver.RegisterAssembly(assembly);
            }
        }
    }
}
