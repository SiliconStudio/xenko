// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.Core;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace SiliconStudio.AssemblyProcessor
{
    public class AssemblyProcessorApp
    {
        private TextWriter log;

        public AssemblyProcessorApp(TextWriter info)
        {
            this.log = info ?? Console.Out;

            SearchDirectories = new List<string>();
            References = new List<string>();
            ReferencesToAdd = new List<string>();
            MemoryReferences = new List<AssemblyDefinition>();
            ModuleInitializer = true;
        }

        public bool AutoNotifyProperty { get; set; }

        public bool ParameterKey { get; set; }

        public bool ModuleInitializer { get; set; }

        public bool SerializationAssembly { get; set; }

        public string DocumentationFile { get; set; }

        public string NewAssemblyName { get; set; }

        internal PlatformType Platform { get; set; }

        public string TargetFramework { get; set; }

        public List<string> SearchDirectories { get; set; }

        public List<string> References { get; set; }

        public List<AssemblyDefinition> MemoryReferences { get; set; }

        public List<string> ReferencesToAdd { get; set; }

        public string SignKeyFile { get; set; }

        public bool UseSymbols { get; set; }

        public bool TreatWarningsAsErrors { get; set; }
        public bool DeleteOutputOnError { get; set; }

        /// <summary>
        /// Should we keep a copy of the original assembly? Useful for debugging.
        /// </summary>
        public bool KeepOriginal { get; internal set; }

        public Action<string, Exception> OnErrorEvent;

        public Action<string> OnInfoEvent;

        public bool Run(string inputFile, string outputFile = null)
        {
            if (inputFile == null) throw new ArgumentNullException("inputFile");
            if (outputFile == null)
            {
                outputFile = inputFile;
            }

            try
            {
                var assemblyResolver = CreateAssemblyResolver();

                var readWriteSymbols = UseSymbols;
                // Double check that 
                var symbolFile = Path.ChangeExtension(inputFile, "pdb");
                if (!File.Exists(symbolFile))
                {
                    readWriteSymbols = false;
                }

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(inputFile, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadSymbols = readWriteSymbols });

                bool modified;

                var result = Run(ref assemblyDefinition, ref readWriteSymbols, out modified);
                if (modified || inputFile != outputFile)
                {
                    // Make sure output directory is created
                    var outputDirectory = Path.GetDirectoryName(outputFile);
                    if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    // Keep the original assembly by adding a .old prefix to the current extension
                    if (KeepOriginal)
                    {
                        var copiedFile = Path.ChangeExtension(inputFile, "old" + Path.GetExtension(inputFile));
                        File.Copy(inputFile, copiedFile, true);
                    }

                    assemblyDefinition.Write(outputFile, new WriterParameters() { WriteSymbols = readWriteSymbols });
                }

                return result;
            }
            catch (Exception e)
            {
                if (DeleteOutputOnError)
                    File.Delete(outputFile);
                OnErrorAction(null, e);
                return false;
            }
        }

        public CustomAssemblyResolver CreateAssemblyResolver()
        {
            var assemblyResolver = new CustomAssemblyResolver();
            assemblyResolver.RemoveSearchDirectory(".");
            foreach (string searchDirectory in SearchDirectories)
                assemblyResolver.AddSearchDirectory(searchDirectory);
            return assemblyResolver;
        }

        public bool Run(ref AssemblyDefinition assemblyDefinition, ref bool readWriteSymbols, out bool modified)
        {
            modified = false;

            try
            {
                var assemblyResolver = (CustomAssemblyResolver)assemblyDefinition.MainModule.AssemblyResolver;

                // Register self
                assemblyResolver.Register(assemblyDefinition);

                var processors = new List<IAssemblyDefinitionProcessor>();

                // We are no longer using it so we are deactivating it for now to avoid processing
                //if (AutoNotifyProperty)
                //{
                //    processors.Add(new NotifyPropertyProcessor());
                //}

                processors.Add(new AddReferenceProcessor(ReferencesToAdd));

                if (ParameterKey)
                {
                    processors.Add(new ParameterKeyProcessor());
                }

                if (NewAssemblyName != null)
                {
                    processors.Add(new RenameAssemblyProcessor(NewAssemblyName));
                }

                //processors.Add(new AsyncBridgeProcessor());

                // Always applies the interop processor
                processors.Add(new InteropProcessor());

                processors.Add(new AssemblyVersionProcessor());

                if (DocumentationFile != null)
                {
                    processors.Add(new GenerateUserDocumentationProcessor(DocumentationFile));
                }

                var roslynExtraCodeProcessor = new RoslynExtraCodeProcessor(SignKeyFile, References, MemoryReferences, log);

                if (SerializationAssembly)
                {
                    processors.Add(new SerializationProcessor(roslynExtraCodeProcessor.SourceCodes.Add));
                }

                processors.Add(roslynExtraCodeProcessor);

                if (ModuleInitializer)
                {
                    processors.Add(new ModuleInitializerProcessor());
                }

                processors.Add(new InitLocalsProcessor());
                processors.Add(new OpenSourceSignProcessor());

                // Check if pdb was actually read
                readWriteSymbols = assemblyDefinition.MainModule.HasDebugHeader;

                // Check if there is already a AssemblyProcessedAttribute (in which case we can skip processing, it has already been done).
                // Note that we should probably also match the command line as well so that we throw an error if processing is different (need to rebuild).
                if (assemblyDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.AssemblyProcessedAttribute"))
                {
                    OnInfoAction($"Assembly [{assemblyDefinition.Name}] has already been processed, skip it.");
                    return true;
                }

                // Register references so that our assembly resolver can use them
                foreach (var reference in References)
                {
                    assemblyResolver.RegisterReference(reference);
                }

                if (SerializationAssembly)
                {
                    // Resave a first version of assembly with [InteralsVisibleTo] for future serialization assembly.
                    // It will be used by serialization assembly compilation.
                    // It's recommended to do it in the original code to avoid this extra step.

                    var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assemblyDefinition);
                    if (mscorlibAssembly == null)
                    {
                        OnErrorAction("Missing reference to mscorlib.dll or System.Runtime.dll in assembly!");
                        return false;
                    }

                    var internalsVisibleToAttribute = mscorlibAssembly.MainModule.GetTypeResolved(typeof(InternalsVisibleToAttribute).FullName);
                    var serializationAssemblyName = assemblyDefinition.Name.Name + ".Serializers";
                    bool internalsVisibleAlreadyApplied = false;

                    // Check if already applied
                    foreach (var customAttribute in assemblyDefinition.CustomAttributes.Where(x => x.AttributeType.FullName == internalsVisibleToAttribute.FullName))
                    {
                        var assemblyName = (string)customAttribute.ConstructorArguments[0].Value;

                        int publicKeyIndex;
                        if ((publicKeyIndex = assemblyName.IndexOf(", PublicKey=", StringComparison.InvariantCulture)) != -1 || (publicKeyIndex = assemblyName.IndexOf(",PublicKey=", StringComparison.InvariantCulture)) != -1)
                        {
                            assemblyName = assemblyName.Substring(0, publicKeyIndex);
                        }

                        if (assemblyName == serializationAssemblyName)
                        {
                            internalsVisibleAlreadyApplied = true;
                            break;
                        }
                    }

                    if (!internalsVisibleAlreadyApplied)
                    {
                        // Apply public key
                        if (assemblyDefinition.Name.HasPublicKey)
                            serializationAssemblyName += ", PublicKey=" + ByteArrayToString(assemblyDefinition.Name.PublicKey);

                        // Add [InteralsVisibleTo] attribute
                        var internalsVisibleToAttributeCtor = assemblyDefinition.MainModule.ImportReference(internalsVisibleToAttribute.GetConstructors().Single());
                        var internalsVisibleAttribute = new CustomAttribute(internalsVisibleToAttributeCtor)
                        {
                            ConstructorArguments =
                            {
                                new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, serializationAssemblyName)
                            }
                        };
                        assemblyDefinition.CustomAttributes.Add(internalsVisibleAttribute);

                        var assemblyFilePath = assemblyDefinition.MainModule.FullyQualifiedName;
                        if (string.IsNullOrEmpty(assemblyFilePath))
                        {
                            assemblyFilePath = Path.ChangeExtension(Path.GetTempFileName(), ".dll");
                        }

                        // Save updated file
                        assemblyDefinition.Write(assemblyFilePath, new WriterParameters() { WriteSymbols = readWriteSymbols });

                        // Reread file (otherwise it seems Mono Cecil is buggy and generate invalid PDB)
                        assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFilePath, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadSymbols = readWriteSymbols });

                        // Check if pdb was actually read
                        readWriteSymbols = assemblyDefinition.MainModule.HasDebugHeader;
                    }
                }

                var assemblyProcessorContext = new AssemblyProcessorContext(assemblyResolver, assemblyDefinition, Platform, log);

                foreach (var processor in processors)
                    modified = processor.Process(assemblyProcessorContext) || modified;

                // Assembly might have been recreated (i.e. il-repack), so let's use it from now on
                assemblyDefinition = assemblyProcessorContext.Assembly;

                if (modified)
                {
                    // In case assembly has been modified,
                    // add AssemblyProcessedAttribute to assembly so that it doesn't get processed again
                    var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assemblyDefinition);
                    if (mscorlibAssembly == null)
                    {
                        OnErrorAction("Missing reference to mscorlib.dll or System.Runtime.dll in assembly!");
                        return false;
                    }

                    var attributeType = mscorlibAssembly.MainModule.GetTypeResolved(typeof (Attribute).FullName);
                    var attributeTypeRef = assemblyDefinition.MainModule.ImportReference(attributeType);
                    var attributeCtorRef = assemblyDefinition.MainModule.ImportReference(attributeType.GetConstructors().Single(x => x.Parameters.Count == 0));
                    var voidType = assemblyDefinition.MainModule.TypeSystem.Void;

                    // Create custom attribute
                    var assemblyProcessedAttributeType = new TypeDefinition("SiliconStudio.Core", "AssemblyProcessedAttribute", TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.Public, attributeTypeRef);

                    // Add constructor (call parent constructor)
                    var assemblyProcessedAttributeConstructor = new MethodDefinition(".ctor", MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, voidType);
                    assemblyProcessedAttributeConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    assemblyProcessedAttributeConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, attributeCtorRef));
                    assemblyProcessedAttributeConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    assemblyProcessedAttributeType.Methods.Add(assemblyProcessedAttributeConstructor);

                    // Add AssemblyProcessedAttribute to assembly
                    assemblyDefinition.MainModule.Types.Add(assemblyProcessedAttributeType);
                    assemblyDefinition.CustomAttributes.Add(new CustomAttribute(assemblyProcessedAttributeConstructor));
                }
            }
            catch (Exception e)
            {
                OnErrorAction(null, e);
                return false;
            }

            return true;
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                result.AppendFormat("{0:x2}", b);
            return result.ToString();
        }

        private void OnErrorAction(string errorMessage, Exception exception = null)
        {
            if (OnErrorEvent == null)
            {
                if (errorMessage != null)
                {
                    log.WriteLine(errorMessage);
                }
                if (exception != null)
                {
                    log.WriteLine(exception.ToString());
                }
            }
            else
            {
                OnErrorEvent(errorMessage, exception);
            }
        }
 
        private void OnInfoAction(string infoMessage)
        {
            if (OnInfoEvent == null)
            {
                log.WriteLine(infoMessage);
            }
            else
            {
                OnInfoEvent(infoMessage);
            }
        }
    }
}
