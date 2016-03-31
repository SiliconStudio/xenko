// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using System.Runtime.Versioning;
using SiliconStudio.AssemblyProcessor.Serializers;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace SiliconStudio.AssemblyProcessor
{
    internal partial class ComplexSerializerCodeGenerator
    {
        private string targetFramework;

        private AssemblyDefinition assembly;
        private string assemblySerializerFactoryClassName;
        private List<TypeReference> referencedAssemblySerializerFactoryTypes = new List<TypeReference>();

        private List<ICecilSerializerDependency> serializerDependencies = new List<ICecilSerializerDependency>();
        private List<ICecilSerializerFactory>  serializerFactories = new List<ICecilSerializerFactory>();
        private CecilSerializerContext cecilSerializerContext;

        //private List<IDataSerializerFactory> serializerFactories = new List<IDataSerializerFactory>();

        public List<ICecilSerializerDependency> SerializerDependencies
        {
            get { return serializerDependencies; }
        }

        public List<ICecilSerializerFactory> SerializerFactories
        {
            get { return serializerFactories; }
        }

        public ComplexSerializerCodeGenerator(IAssemblyResolver assemblyResolver, AssemblyDefinition assembly, TextWriter log)
        {
            this.assembly = assembly;
            this.assemblySerializerFactoryClassName = Utilities.BuildValidClassName(assembly.Name.Name) + "SerializerFactory";

            // Register referenced assemblies serializer factory, so that we can call them recursively
            foreach (var referencedAssemblyName in assembly.MainModule.AssemblyReferences)
            {
                try
                {
                    var referencedAssembly = assembly.MainModule.AssemblyResolver.Resolve(referencedAssemblyName);

                    var assemblySerializerFactoryType = GetSerializerFactoryType(referencedAssembly);
                    if (assemblySerializerFactoryType != null)
                        referencedAssemblySerializerFactoryTypes.Add(assemblySerializerFactoryType);
                }
                catch (AssemblyResolutionException)
                {
                    continue;
                }
            }

            // Find target framework and replicate it for serializer assembly.
            var targetFrameworkAttribute = assembly.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
            if (targetFrameworkAttribute != null)
            {
                targetFramework = "\"" + (string)targetFrameworkAttribute.ConstructorArguments[0].Value + "\"";
                var frameworkDisplayNameField = targetFrameworkAttribute.Properties.FirstOrDefault(x => x.Name == "FrameworkDisplayName");
                if (frameworkDisplayNameField.Name != null)
                {
                    targetFramework += ", FrameworkDisplayName=\"" + (string)frameworkDisplayNameField.Argument.Value + "\"";
                }
            }

            // Prepare serializer processors
            cecilSerializerContext = new CecilSerializerContext(assembly, log);
            var processors = new List<ICecilSerializerProcessor>();

            // Import list of serializer registered by referenced assemblies
            processors.Add(new ReferencedAssemblySerializerProcessor());

            // Generate serializers for types tagged as serializable
            processors.Add(new CecilComplexClassSerializerProcessor());

            // Generate serializers for PropertyKey and ParameterKey
            processors.Add(new PropertyKeySerializerProcessor());

            // Update Engine (with AnimationData<T>)
            processors.Add(new UpdateEngineProcessor());

            // Profile serializers
            processors.Add(new ProfileSerializerProcessor());

            // Data contract aliases
            processors.Add(new DataContractAliasProcessor());

            // Apply each processor
            foreach (var processor in processors)
                processor.ProcessSerializers(cecilSerializerContext);
        }

        internal static TypeDefinition GetSerializerFactoryType(AssemblyDefinition referencedAssembly)
        {
            var assemblySerializerFactoryAttribute =
                referencedAssembly.CustomAttributes.FirstOrDefault(
                    x =>
                        x.AttributeType.FullName ==
                        "SiliconStudio.Core.Serialization.Serializers.AssemblySerializerFactoryAttribute");

            // No serializer factory?
            if (assemblySerializerFactoryAttribute == null)
                return null;

            var typeReference = (TypeReference)assemblySerializerFactoryAttribute.Fields.Single(x => x.Name == "Type").Argument.Value;
            if (typeReference == null)
                return null;

            return typeReference.Resolve();
        }
    }
}
