// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace SiliconStudio.AssemblyProcessor
{
    internal class AssemblyScanProcessor : IAssemblyDefinitionProcessor
    {
        private static readonly string attributeUsageTypeName = typeof(AttributeUsageAttribute).FullName;
        private SerializationProcessor.RegisterSourceCode sourceCodeRegisterAction;

        public AssemblyScanProcessor(SerializationProcessor.RegisterSourceCode sourceCodeRegisterAction)
        {
            this.sourceCodeRegisterAction = sourceCodeRegisterAction;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            var assemblyScanCodeGenerator = new AssemblyScanCodeGenerator(context.Assembly);
            foreach (var type in context.Assembly.MainModule.GetAllTypes())
            {
                // Ignore interface types as well as types with generics
                // Note: we could support generic types at some point but we probably need
                //       to get static generic instantiation type list from serializer code generator
                if (type.IsInterface || type.HasGenericParameters)
                    continue;

                var currentType = type;
                // Scan type and parent types
                while (currentType != null)
                {
                    // Scan interfaces
                    foreach (var @interface in currentType.Interfaces)
                    {
                        ScanAttributes(context.Log, assemblyScanCodeGenerator, @interface.InterfaceType, type);
                    }

                    ScanAttributes(context.Log, assemblyScanCodeGenerator, currentType, type);
                    currentType = currentType.BaseType?.Resolve();
                }
            }

            if (assemblyScanCodeGenerator.HasScanTypes)
            {
                // Generate code
                sourceCodeRegisterAction(assemblyScanCodeGenerator.TransformText(), "AssemblyScan");
            }

            return assemblyScanCodeGenerator.HasScanTypes;
        }

        private static void ScanAttributes(TextWriter log, AssemblyScanCodeGenerator codeGenerator, TypeReference scanType, TypeDefinition type)
        {
            foreach (var attribute in scanType.Resolve().CustomAttributes)
            {
                // Check if scanned type has any AssemblyScanAttribute attribute
                if (attribute.AttributeType.FullName == "SiliconStudio.Core.Reflection.AssemblyScanAttribute")
                {
                    RegisterType(log, codeGenerator, type, scanType);
                }

                // Check if the attribute type has any AssemblyScanAttribute attribute
                // This allows to create custom attributes and scan for them
                foreach (var attributeAttribute in attribute.AttributeType.Resolve().CustomAttributes)
                {
                    var hasAssemblyScanAttribute = false;
                    if (attributeAttribute.AttributeType.FullName == "SiliconStudio.Core.Reflection.AssemblyScanAttribute")
                    {
                        hasAssemblyScanAttribute = true;
                    }
                    else if (attributeAttribute.AttributeType.FullName == attributeUsageTypeName)
                    {
                        // If AttributeUsage has Inherited = false, let's skip right away if we are not processing main type
                        if (scanType != type && attributeAttribute.HasProperties
                            && attributeAttribute.Properties.FirstOrDefault(x => x.Name == nameof(AttributeUsageAttribute.Inherited)).Argument.Value as bool? == false)
                            break;
                    }

                    if (hasAssemblyScanAttribute)
                    {
                        RegisterType(log, codeGenerator, type, attribute.AttributeType);
                    }
                }
            }
        }

        private static void RegisterType(TextWriter log, AssemblyScanCodeGenerator codeGenerator, TypeDefinition type, TypeReference scanType)
        {
            // Nested type needs to be either public or internal otherwise we can't access them from other classes
            if (type.IsNested && !type.IsNestedPublic && !type.IsNestedAssembly)
            {
                log.WriteLine($"{nameof(AssemblyScanProcessor)}: Can't register type [{type}] for scan type [{scanType}] because it is a nested private type");
                return;
            }

            codeGenerator.Register(type, scanType);
        }
    }
}
