// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

using SiliconStudio.AssemblyProcessor;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.VisualStudio.Commands.DataGenerator
{
    public partial class DataConverterGenerator
    {
        private Dictionary<TypeDefinition, DataTypeInfo> processedTypes = new Dictionary<TypeDefinition, DataTypeInfo>();
        private Dictionary<TypeDefinition, DataConverterInfo> processedConverterTypes = new Dictionary<TypeDefinition, DataConverterInfo>();
        private DataPropertyTypeVisitor dataPropertyTypeVisitor;
        private AssemblyDefinition assembly;

        private TypeDefinition listType;
        private TypeDefinition dictionaryType;

        private string currentNamespace = string.Empty;

        public DataConverterGenerator(IAssemblyResolver assemblyResolver, AssemblyDefinition assembly)
        {
            this.assembly = assembly;

            // Early exit if Serialization assembly is not referenced.
            AssemblyDefinition mscorlibAssembly;
            try
            {
                // In case assembly has been modified,
                // add AssemblyProcessedAttribute to assembly so that it doesn't get processed again
                mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
                if (mscorlibAssembly == null)
                    throw new InvalidOperationException("Missing mscorlib.dll from assembly");
            }
            catch
            {
                return;
            }

            listType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(List<>).FullName);
            dictionaryType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(Dictionary<,>).FullName);

            this.dataPropertyTypeVisitor = new DataPropertyTypeVisitor(this);

            // Process types with DataConverterAttribute
            foreach (var type in assembly.EnumerateTypes())
            {
                // Generic types are not supported.
                if(type.HasGenericParameters)
                    continue;

                // Do not allow nested type to be generic (not properly supported, since it could be the enclosing type which is generic)
                if (!(type.IsPublic || (type.IsNestedPublic && !type.HasGenericParameters)))
                    continue;

                var dataInfo = this.GenerateDataTypeInfo(type);
                if (dataInfo != null)
                {
                    var dataConverterInfo = new DataConverterInfo
                        {
                            Generate = (dataInfo.Flags & DataTypeFlags.Generated) != 0,
                            DataInfo = dataInfo,
                            Type = this.GetDataConverterType(type),
                        };

                    this.processedTypes.Add(type, dataInfo);
                    this.processedConverterTypes.Add(type, dataConverterInfo);
                }

                var additionalConverterAttribute = GetAdditionalDataAttribute(type);
                if (additionalConverterAttribute != null)
                {
                    var baseTypeArgument = GetAdditionalBaseType(type);
                    var dataConverterInfo = new DataConverterInfo
                        {
                            DataInfo = this.GenerateDataTypeInfo(baseTypeArgument),
                            Type = this.GetDataConverterType(type),
                        };

                    foreach (var namedArgument in additionalConverterAttribute.Properties)
                    {
                        switch (namedArgument.Name)
                        {
                            case "AutoGenerate":
                                dataConverterInfo.Generate = (bool)namedArgument.Argument.Value;
                                break;
                        }
                    }

                    this.processedConverterTypes.Add(type, dataConverterInfo);
                }
            }
        }

        private DataTypeInfo GenerateDataTypeInfo(TypeDefinition type)
        {
            DataTypeInfo existingDataTypeInfo;
            if (this.processedTypes.TryGetValue(type, out existingDataTypeInfo))
                return existingDataTypeInfo;

            var flags = GetDataTypeFlags(type);
            if (flags == null || ((flags.Value & DataTypeFlags.NoDataType) != 0))
                return null;

            // Add properties
            var properties = new List<DataProperty>();
            if ((flags.Value & DataTypeFlags.Generated) != 0)
            {
                properties.AddRange(this.EnumerateProperties(type));
            }

            return new DataTypeInfo
                {
                    Type = this.GetDataType(type),
                    BaseType = (flags.Value & DataTypeFlags.Generated) != 0 ? this.GetDataType(type.BaseType.Resolve()) : null,
                    Properties = properties,
                    Flags = flags.Value,
                };
        }

        /// <summary>
        /// Properly opens/closes namespace.
        /// </summary>
        /// <param name="newNamespace">The new namespace.</param>
        private void ChangeNamespace(string newNamespace)
        {
            if (this.currentNamespace != newNamespace)
            {
                if (this.currentNamespace != string.Empty)
                {
                    this.Write("}\r\n");
                    this.Write("\r\n");
                }

                this.currentNamespace = newNamespace;

                if (this.currentNamespace != string.Empty)
                {
                    this.Write("namespace {0}\r\n", this.currentNamespace);
                    this.Write("{\r\n");
                }
            }
        }

        private TypeReference GetDataConverterType(TypeDefinition type)
        {
            // Since we don't write the IL code back but use a C# code writer, no need for it to have real module/scopes.
            return new TypeReference(type.Namespace + ".Data", type.Name + "DataConverter", type.Module, type.Scope, false);
        }

        private TypeReference GetDataType(TypeDefinition type)
        {
            var dataConverterAttribute = GetDataConverterAttribute(type);
            if (dataConverterAttribute == null)
                return null;

            var flags = GetDataTypeFlags(type);
            if (flags != null && ((flags.Value & DataTypeFlags.NoDataType) != 0))
                return type;

            var namedArgument = dataConverterAttribute.Properties.FirstOrDefault(x => x.Name == "DataTypeName");
            var name = (namedArgument.Name != null) ? (string)namedArgument.Argument.Value : type.Name + "Data";

            var lastDot = name.LastIndexOf('.');
            var @namespace = lastDot != -1 ? name.Substring(lastDot + 1) : type.Namespace + ".Data";

            // Since we don't write the IL code back but use a C# code writer, no need for it to have real module/scopes.
            return new TypeReference(@namespace, name, type.Module, type.Scope, false);
        }

        private CustomAttribute GetAdditionalDataAttribute(TypeDefinition type)
        {
            // Is it a DataAdditionalConverterAttribute?
            // If yes, resolve through its BaseType.
            return GetDataConverterAttribute(type, typeof(DataAdditionalConverterAttribute).FullName);
        }

        private TypeDefinition GetAdditionalBaseType(TypeDefinition type)
        {
            // Is it a DataAdditionalConverterAttribute?
            // If yes, resolve through its BaseType.
            var additionalConverterAttribute = GetAdditionalDataAttribute(type);
            if (additionalConverterAttribute != null)
            {
                var baseTypeArgumentReference = (TypeReference)additionalConverterAttribute.ConstructorArguments[0].Value;
                var baseTypeArgument = baseTypeArgumentReference.Resolve();

                return baseTypeArgument;
            }
            return null;
        }

        private static CustomAttribute GetDataConverterAttribute(TypeDefinition type, string name = "SiliconStudio.Core.Serialization.Converters.DataConverterAttribute")
        {
            // Check if DataConverter is set on this class, without any argument
            // We either include only DataConverterAttribute without arguments (if defined == false) or all of them (defined == true)
            var dataConverterAttribute =
                type.CustomAttributes.FirstOrDefault(
                    x => x.AttributeType.FullName == name);
            if (dataConverterAttribute != null)
                return dataConverterAttribute;

            return null;
        }

        private static DataTypeFlags? GetDataTypeFlags(TypeDefinition type)
        {
            var dataConverterAttribute = GetDataConverterAttribute(type);

            if (dataConverterAttribute == null)
                return null;

            var flags = DataTypeFlags.None;

            foreach (var namedArgument in dataConverterAttribute.Properties)
            {
                switch (namedArgument.Name)
                {
                    case "DataType":
                        if (!(bool)namedArgument.Argument.Value)
                            flags |= DataTypeFlags.NoDataType;
                        break;
                    case "AutoGenerate":
                        if ((bool)namedArgument.Argument.Value)
                            flags |= DataTypeFlags.Generated;
                        break;
                    case "ContentReference":
                        if ((bool)namedArgument.Argument.Value)
                            flags |= DataTypeFlags.ContentReference;
                        break;
                    case "CustomConvertToData":
                        if ((bool)namedArgument.Argument.Value)
                            flags |= DataTypeFlags.CustomConvertToData;
                        break;
                    case "CustomConvertFromData":
                        if ((bool)namedArgument.Argument.Value)
                            flags |= DataTypeFlags.CustomConvertFromData;
                        break;
                }
            }

            // Check if type is an EntityComponent (or inherits from one
            while (type != null)
            {
                if (type.FullName == typeof(EntityModel.EntityComponent).FullName)
                    flags |= DataTypeFlags.EntityComponent;

                // TODO: Resolve with ResolveGenericsVisitor
                type = type.BaseType != null ? type.BaseType.Resolve() : null;
            }

            return flags;
        }

        private IEnumerable<DataProperty> EnumerateProperties(TypeDefinition type)
        {
            // Only deal with writable public properties
            foreach (var property in type.Properties)
            {
                if (!property.CustomAttributes.Any(x => x.AttributeType.Name == typeof(DataMemberConvertAttribute).Name))
                    continue;

                var dataProperty = CreateDataProperty(property.Name, property.PropertyType, property.SetMethod != null && (property.SetMethod.IsPublic || property.SetMethod.IsAssembly));
                if (dataProperty != null)
                    yield return dataProperty;
            }

            // and fields
            foreach (var field in type.Fields)
            {
                if (!field.CustomAttributes.Any(x => x.AttributeType.Name == typeof(DataMemberConvertAttribute).Name))
                    continue;

                var dataProperty = CreateDataProperty(field.Name, field.FieldType, true);
                if (dataProperty != null)
                    yield return dataProperty;
            }
        }

        private DataProperty CreateDataProperty(string memberName, TypeReference memberType, bool hasPublicSetAccessor)
        {
            var originalTypeReference = memberType;
            var dataType = this.dataPropertyTypeVisitor.VisitDynamic(originalTypeReference);

            var dataProperty = new DataProperty
                {
                    HasPublicSetAccessor = hasPublicSetAccessor,
                    OriginalType = originalTypeReference,
                    DataType = dataType,
                    Name = memberName,
                };

            // In some case (property has no public setter and its type is IList/IDictionary,
            // we want to create it automatically in the Data type as well).
            // Example: IList<A> Member { get; private set; } will result in IList<AData> Member = new List<AData>();
            if (!dataProperty.HasPublicSetAccessor
                && dataType.IsGenericInstance)
            {
                var dataTypeGenericInstance = (GenericInstanceType)dataType;
                var elementType = dataType.GetElementType().Resolve();

                foreach (var genericPattern in new[]
                {
                    new {GenericType = typeof(IList<>), InterfaceInitializerType = listType},
                    new {GenericType = typeof(IDictionary<,>), InterfaceInitializerType = dictionaryType}
                })
                {
                    // Is it IList<> or does it implement IList<>?
                    if (elementType.FullName == genericPattern.GenericType.FullName)
                    {
                        // IList<T> => use List<T>
                        dataProperty.InitializerType =
                            dataTypeGenericInstance.ChangeGenericInstanceType(genericPattern.InterfaceInitializerType,
                                dataTypeGenericInstance.GenericArguments);
                        break;
                    }
                    if (
                        elementType.Interfaces.Any(
                            x => x.IsGenericInstance && x.GetElementType().FullName == genericPattern.GenericType.FullName))
                    {
                        // Implements IList<T>, if it has an empty constructor let's use it
                        if (elementType.Methods.Any(x => x.IsConstructor && x.Parameters.Count == 0))
                        {
                            dataProperty.InitializerType = dataType;
                            break;
                        }
                    }
                }
            }

            return dataProperty;
        }

        class DataProperty
        {
            public bool HasPublicSetAccessor;
            public TypeReference InitializerType;
            public TypeReference OriginalType;
            public TypeReference DataType;
            public string Name;
        }

        class DataPropertyTypeVisitor : CecilTypeReferenceVisitor
        {
            private DataConverterGenerator dataConverterGenerator;
            private TypeReference contentReferenceType;
            private TypeReference entityComponentReferenceType;

            public DataPropertyTypeVisitor(DataConverterGenerator dataConverterGenerator)
            {
                this.dataConverterGenerator = dataConverterGenerator;
                var coreSerializationAssembly = dataConverterGenerator.assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Core.Serialization");
                this.contentReferenceType = dataConverterGenerator.assembly.MainModule.Import(coreSerializationAssembly.MainModule.GetTypeResolved("SiliconStudio.Core.Serialization.ContentReference`1"));
            }

            public TypeReference TryTransformToDataType(TypeReference type)
            {
                var resolvedType = type.Resolve();
                var dataType = this.dataConverterGenerator.GetDataType(resolvedType);
                if (dataType == null)
                {
                    // Try to go through DataAdditionalConverterAttribute
                    resolvedType = this.dataConverterGenerator.GetAdditionalBaseType(resolvedType);
                    if (resolvedType == null)
                        return type;
                    dataType = this.dataConverterGenerator.GetDataType(resolvedType);
                }

                var flags = GetDataTypeFlags(resolvedType).Value;

                // Special case: EntityComponentReference<>
                // TODO: Make this system more flexible instead of hardcoding it here
                if ((flags & DataTypeFlags.EntityComponent) != 0)
                {
                    if (entityComponentReferenceType == null)
                    {
                        var engineAssembly = dataConverterGenerator.assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Paradox.Engine");
                        entityComponentReferenceType = dataConverterGenerator.assembly.MainModule.Import(engineAssembly.MainModule.GetTypeResolved(typeof(EntityComponentReference<>).FullName));
                    }

                    // Create a EntityComponentReference<> of the source type (not the data type, since we only care about the PropertyKey<>)
                    var result = new GenericInstanceType(entityComponentReferenceType);
                    result.GenericArguments.Add(type);
                    dataType = result;
                }
                else if ((flags & DataTypeFlags.ContentReference) != 0)
                {
                    // If there is a data type, transform it to a ContentReference<> of this data type.
                    var result = new GenericInstanceType(this.contentReferenceType);
                    result.GenericArguments.Add(dataType);
                    dataType = result;
                }

                return dataType;
            }

            public override TypeReference Visit(TypeReference type)
            {
                type = this.TryTransformToDataType(type);

                return base.Visit(type);
            }
        }

        class DataTypeInfo
        {
            public TypeReference Type;
            public TypeReference BaseType;
            public DataTypeFlags Flags;
            public List<DataProperty> Properties;
        }

        class DataConverterInfo
        {
            public bool Generate;
            public DataTypeInfo DataInfo;
            public TypeReference Type;
        }

        [Flags]
        enum DataTypeFlags
        {
            None = 0,
            Generated = 1,
            ContentReference = 2,
            CustomConvertToData = 4,
            CustomConvertFromData = 8,
            NoDataType = 16, // No need for a separate data type (useful to only embed object in a ContentReference as is)
            EntityComponent = 32,
        }
    }
}