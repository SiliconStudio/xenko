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
        public AssemblyProcessorApp()
        {
            SearchDirectories = new List<string>();
            SerializationProjectReferences = new List<string>();
            ModuleInitializer = true;
        }

        public bool AutoNotifyProperty { get; set; }

        public bool ParameterKey { get; set; }

        public bool ModuleInitializer { get; set; }

        public bool SerializationAssembly { get; set; }

        public bool GenerateUserDocumentation { get; set; }

        public string NewAssemblyName { get; set; }

        public PlatformType Platform { get; set; }

        public string TargetFramework { get; set; }

        public List<string> SearchDirectories { get; set; }

        public List<string> SerializationProjectReferences { get; set; } 

        public string SignKeyFile { get; set; }

        public bool UseSymbols { get; set; }

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

                    assemblyDefinition.Write(outputFile, new WriterParameters() { WriteSymbols = readWriteSymbols });
                }

                return result;
            }
            catch (Exception e)
            {
                OnErrorAction(e.Message, e);
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

                var processors = new List<IAssemblyDefinitionProcessor>();

                // We are no longer using it so we are deactivating it for now to avoid processing
                //if (AutoNotifyProperty)
                //{
                //    processors.Add(new NotifyPropertyProcessor());
                //}

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

                if (SerializationAssembly)
                {
                    processors.Add(new SerializationProcessor(SignKeyFile, SerializationProjectReferences));
                }

                if (GenerateUserDocumentation)
                {
                    processors.Add(new GenerateUserDocumentationProcessor(assemblyDefinition.MainModule.FullyQualifiedName));
                }

                if (ModuleInitializer)
                {
                    processors.Add(new ModuleInitializerProcessor());
                }

                processors.Add(new OpenSourceSignProcessor());

                // Check if pdb was actually read
                readWriteSymbols = assemblyDefinition.MainModule.HasDebugHeader;

                // Check if there is already a AssemblyProcessedAttribute (in which case we can skip processing, it has already been done).
                // Note that we should probably also match the command line as well so that we throw an error if processing is different (need to rebuild).
                if (assemblyDefinition.CustomAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.AssemblyProcessedAttribute"))
                {
                    OnInfoAction("Assembly has already been processed, skip it.");
                    return true;
                }

                var targetFrameworkAttribute = assemblyDefinition.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
                var targetFramework = targetFrameworkAttribute != null ? (string)targetFrameworkAttribute.ConstructorArguments[0].Value : null;

                // Special handling for MonoAndroid
                // Default frameworkFolder
                var frameworkFolder = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\");

                switch (Platform)
                {
                    case PlatformType.Android:
                    {
                        if (string.IsNullOrEmpty(TargetFramework))
                        {
                            throw new InvalidOperationException("Expecting option target framework for Android");
                        }

                        var monoAndroidPath = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Reference Assemblies\Microsoft\Framework\MonoAndroid");
                        frameworkFolder = Path.Combine(monoAndroidPath, "v1.0");
                        var additionalFrameworkFolder = Path.Combine(monoAndroidPath, TargetFramework);
                        assemblyResolver.AddSearchDirectory(additionalFrameworkFolder);
                        assemblyResolver.AddSearchDirectory(frameworkFolder);
                        break;
                    }

                    case PlatformType.iOS:
                    {
                        if (string.IsNullOrEmpty(TargetFramework))
                        {
                            throw new InvalidOperationException("Expecting option target framework for iOS");
                        }

                        var monoTouchPath = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Reference Assemblies\Microsoft\Framework\Xamarin.iOS");
                        frameworkFolder = Path.Combine(monoTouchPath, "v1.0");
                        var additionalFrameworkFolder = Path.Combine(monoTouchPath, TargetFramework);
                        assemblyResolver.AddSearchDirectory(additionalFrameworkFolder);
                        assemblyResolver.AddSearchDirectory(frameworkFolder);

                        break;
                    }

                    case PlatformType.WindowsStore:
                    {
                        if (string.IsNullOrEmpty(TargetFramework))
                        {
                            throw new InvalidOperationException("Expecting option target framework for WindowsStore");
                        }

                        frameworkFolder = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Reference Assemblies\Microsoft\Framework\.NETCore", TargetFramework);
                        assemblyResolver.AddSearchDirectory(frameworkFolder);

                        // Add path to look for WinRT assemblies (Windows.winmd)
                        var windowsAssemblyPath = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Windows Kits\8.1\References\CommonConfiguration\Neutral\", "Windows.winmd");
                        var windowsAssembly = AssemblyDefinition.ReadAssembly(windowsAssemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadSymbols = false });
                        assemblyResolver.Register(windowsAssembly);

                        break;
                    }

                    case PlatformType.WindowsPhone:
                    {
                        if (string.IsNullOrEmpty(TargetFramework))
                        {
                            throw new InvalidOperationException("Expecting option target framework for WindowsPhone");
                        }

                        // Note: v8.1 is hardcoded because we currently receive v4.5.x as TargetFramework (different from TargetPlatformVersion)
                        frameworkFolder = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Reference Assemblies\Microsoft\Framework\WindowsPhoneApp", "v8.1");
                        assemblyResolver.AddSearchDirectory(frameworkFolder);

                        // Add path to look for WinRT assemblies (Windows.winmd)
                        var windowsAssemblyPath = Path.Combine(CecilExtensions.ProgramFilesx86(), @"Windows Phone Kits\8.1\References\CommonConfiguration\Neutral\", "Windows.winmd");
                        var windowsAssembly = AssemblyDefinition.ReadAssembly(windowsAssemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadSymbols = false });
                        assemblyResolver.Register(windowsAssembly);

                        break;
                    }
                }

                if (SerializationAssembly)
                {
                    // Resave a first version of assembly with [InteralsVisibleTo] for future serialization assembly.
                    // It will be used by serialization assembly compilation.
                    // It's recommended to do it in the original code to avoid this extra step.

                    var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assemblyDefinition);
                    var stringType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(string).FullName);
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
                        var internalsVisibleToAttributeCtor = assemblyDefinition.MainModule.Import(internalsVisibleToAttribute.GetConstructors().Single());
                        var internalsVisibleAttribute = new CustomAttribute(internalsVisibleToAttributeCtor)
                        {
                            ConstructorArguments =
                            {
                                new CustomAttributeArgument(assemblyDefinition.MainModule.Import(stringType), serializationAssemblyName)
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

                var assemblyProcessorContext = new AssemblyProcessorContext(assemblyResolver, assemblyDefinition, Platform);

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
                        OnErrorAction("Missing mscorlib.dll from assembly");
                        return false;
                    }

                    var attributeType = mscorlibAssembly.MainModule.GetTypeResolved(typeof (Attribute).FullName);
                    var attributeTypeRef = assemblyDefinition.MainModule.Import(attributeType);
                    var attributeCtorRef = assemblyDefinition.MainModule.Import(attributeType.GetConstructors().Single(x => x.Parameters.Count == 0));
                    var voidType = assemblyDefinition.MainModule.Import(mscorlibAssembly.MainModule.GetTypeResolved("System.Void"));

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
                OnErrorAction(e.Message, e);
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
                var builder = new StringBuilder();
                builder.AppendLine(errorMessage);
                if (exception != null)
                {
                    builder.AppendLine(exception.ToString());
                    var nextE = exception;
                    for (int index = 0; nextE != null; nextE = nextE.InnerException, index++)
                        builder.AppendFormat("{0}{1}", string.Concat(Enumerable.Repeat(" ", index)), nextE.Message).AppendLine();
                    builder.AppendLine();
                }

                Console.WriteLine(builder);
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
                Console.WriteLine(infoMessage);
            }
            else
            {
                OnInfoEvent(infoMessage);
            }
        }
    }
}