// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.AssemblyProcessor.Serializers
{
    /// <summary>
    /// Fill <see cref="CecilSerializerContext.SerializableTypes"/> with serializable types handled by referenced assemblies.
    /// </summary>
    class ReferencedAssemblySerializerProcessor : ICecilSerializerProcessor
    {
        HashSet<AssemblyDefinition> processedAssemblies = new HashSet<AssemblyDefinition>();

        public void ProcessSerializers(CecilSerializerContext context)
        {
            ProcessDataSerializerGlobalAttributes(context, context.Assembly, true);
        }

        private void ProcessDataSerializerGlobalAttributes(CecilSerializerContext context, AssemblyDefinition assembly, bool local)
        {
            // Already processed?
            if (!processedAssemblies.Add(assembly))
                return;

            // TODO: Add a flag for ComplexSerializer and transmit it properly (it needs different kind of analysis)

            // Let's recurse over referenced assemblies
            foreach (var referencedAssemblyName in assembly.MainModule.AssemblyReferences.ToArray())
            {
                // Avoid processing system assemblies
                // TODO: Scan what is actually in framework folders
                if (referencedAssemblyName.Name == "mscorlib" || referencedAssemblyName.Name.StartsWith("System")
                    || referencedAssemblyName.FullName.Contains("PublicKeyToken=31bf3856ad364e35")) // Signed with Microsoft public key (likely part of system libraries)
                    continue;

                try
                {
                    var referencedAssembly = context.Assembly.MainModule.AssemblyResolver.Resolve(referencedAssemblyName);

                    ProcessDataSerializerGlobalAttributes(context, referencedAssembly, false);
                }
                catch (AssemblyResolutionException)
                {
                    continue;
                }
            }

            // Find DataSerializer attribute on assembly and/or types
            foreach (var dataSerializerAttribute in
                assembly.CustomAttributes.Concat(assembly.MainModule.Types.SelectMany(x => x.CustomAttributes)).Where(
                    x => x.AttributeType.FullName == "SiliconStudio.Core.Serialization.DataSerializerGlobalAttribute")
                    .OrderBy(x => x.ConstructorArguments[0].Value != null ? -1 : 1)) // Order so that we first have the ones which don't require us to go through GenerateSerializer
            {
                var dataSerializerType = (TypeReference)dataSerializerAttribute.ConstructorArguments[0].Value;
                var dataType = (TypeReference)dataSerializerAttribute.ConstructorArguments[1].Value;
                var mode = (DataSerializerGenericMode)dataSerializerAttribute.ConstructorArguments[2].Value;
                var inherited = (bool)dataSerializerAttribute.ConstructorArguments[3].Value;
                var complexSerializer = (bool)dataSerializerAttribute.ConstructorArguments[4].Value;
                var profile = dataSerializerAttribute.Properties.Where(x => x.Name == "Profile").Select(x => (string)x.Argument.Value).FirstOrDefault() ?? "Default";

                if (dataType == null)
                {
                    if (mode == DataSerializerGenericMode.None)
                        dataType = FindSerializerDataType(dataSerializerType);
                    else
                        throw new InvalidOperationException("Can't deduce data serializer type for generic types.");
                }

                CecilSerializerContext.SerializableTypeInfo serializableTypeInfo;

                if (dataSerializerType == null)
                {
                    // TODO: We should avoid calling GenerateSerializer now just to have the dataSerializerType (we should do so only in a second step)
                    serializableTypeInfo = context.GenerateSerializer(dataType, profile: profile);
                    if (serializableTypeInfo == null)
                        throw new InvalidOperationException(string.Format("Can't find serializer for type {0}", dataType));
                    serializableTypeInfo.Local = local;
                    serializableTypeInfo.ExistingLocal = local;
                    dataSerializerType = serializableTypeInfo.SerializerType;
                }
                else
                {
                    // Add it to list of serializable types
                    serializableTypeInfo = new CecilSerializerContext.SerializableTypeInfo(dataSerializerType, local, mode) { ExistingLocal = local, Inherited = inherited, ComplexSerializer = complexSerializer };
                    context.AddSerializableType(dataType, serializableTypeInfo, profile);
                }
            }
        }

        public static TypeReference FindSerializerDataType(TypeReference dataSerializerType)
        {
            // Find "DataSerializer<T>" base and its dataType (T)
            TypeReference dataType = null;
            var dataSerializerTypeCurrent = dataSerializerType;
            while (dataSerializerTypeCurrent != null)
            {
                var genericInstanceType = dataSerializerTypeCurrent as GenericInstanceType;
                if (genericInstanceType != null)
                {
                    if (genericInstanceType.ElementType.FullName == "SiliconStudio.Core.Serialization.DataSerializer`1")
                    {
                        dataType = genericInstanceType.GenericArguments[0];
                        break;
                    }
                }

                var baseType = ResolveGenericsVisitor.Process(dataSerializerTypeCurrent, dataSerializerTypeCurrent.Resolve().BaseType);

                dataSerializerTypeCurrent = baseType;
            }

            if (dataType == null)
                throw new InvalidOperationException(string.Format("Could not determine data type for {0}.", dataSerializerType));
            return dataType;
        }
    }
}