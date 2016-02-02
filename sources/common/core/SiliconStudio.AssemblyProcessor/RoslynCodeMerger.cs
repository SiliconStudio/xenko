// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Various helper functions to compile low-level "complex" serializer.
    /// </summary>
    public static class RoslynCodeMerger
    {
        public static AssemblyDefinition GenerateRoslynAssembly(BaseAssemblyResolver assemblyResolver, AssemblyDefinition assembly, string serializationAssemblyLocation, string signKeyFile, List<string> references, List<AssemblyDefinition> memoryReferences, TextWriter log, IEnumerable<string> sourceCodes)
        {
            var syntaxTrees = sourceCodes.Select(x => CSharpSyntaxTree.ParseText(x));

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
            foreach (var reference in references)
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
            metadataReferences.Add(CreateMetadataReference(assemblyResolver, assembly));

            // In case SiliconStudio.Core was not referenced, let's add it.
            if (assembly.Name.Name != "SiliconStudio.Core" && !references.Any(x => string.Compare(Path.GetFileNameWithoutExtension(x), "SiliconStudio.Core", StringComparison.OrdinalIgnoreCase) == 0))
            {
                metadataReferences.Add(CreateMetadataReference(assemblyResolver, assemblyResolver.Resolve("SiliconStudio.Core")));
            }

            // Create roslyn compilation object
            var assemblyName = assembly.Name.Name + ".Serializers";
            var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, metadataReferences, compilerOptions);

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

            var repackOptions = new ILRepacking.RepackOptions(new string[0])
            {
                OutputFile = assembly.MainModule.FullyQualifiedName,
                DebugInfo = true,
                CopyAttributes = true,
                AllowMultipleAssemblyLevelAttributes = true,
                XmlDocumentation = false,
                NoRepackRes = true,
                InputAssemblies = new[] { serializationAssemblyLocation },
                SearchDirectories = assemblyResolver.GetSearchDirectories(),
                SearchAssemblies = references,
            };

            // Run ILMerge
            var merge = new ILRepacking.ILRepack(repackOptions)
            {
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

        public static string GenerateRolsynAssemblyLocation(string assemblyLocation)
        {
            if (!Regex.IsMatch(assemblyLocation, ".dll|.exe", RegexOptions.IgnoreCase))
                throw new InvalidOperationException();

            // For historic reason, it is named *.Serializers.dll
            return Regex.Replace(assemblyLocation, ".dll|.exe", ".Serializers.dll", RegexOptions.IgnoreCase);
        }
    }
}
