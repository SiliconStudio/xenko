// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    internal partial class ComplexClassSerializerGenerator
    {
        private TypeDefinition type;
        private bool hasParentSerializer;
        private static HashSet<string> forbiddenKeywords;
        private static HashSet<IMemberDefinition> ignoredMembers; 

        static ComplexClassSerializerGenerator()
        {
            ignoredMembers = new HashSet<IMemberDefinition>();

            forbiddenKeywords = new HashSet<string>(new[]
                { "obj", "stream", "mode",
                    "abstract", "event", "new", "struct",
                    "as", "explicit", "null", "switch",
                    "base", "extern", "object", "this",
                    "bool", "false", "operator", "throw",
                    "break", "finally", "out", "true",
                    "byte", "fixed", "override", "try",
                    "case", "float", "params", "typeof",
                    "catch", "for", "private", "uint",
                    "char", "foreach", "protected", "ulong",
                    "checked", "goto", "public", "unchecked",
                    "class", "if", "readonly", "unsafe",
                    "const", "implicit", "ref", "ushort",
                    "continue", "in", "return", "using",
                    "decimal", "int", "sbyte", "virtual",
                    "default", "interface", "sealed", "volatile",
                    "delegate", "internal", "short", "void",
                    "do", "is", "sizeof", "while",
                    "double", "lock", "stackalloc",
                    "else", "long", "static",
                    "enum", "namespace", "string" });
        }

        public ComplexClassSerializerGenerator(TypeDefinition type, bool hasParentSerializer)
        {
            this.type = type;
            this.hasParentSerializer = hasParentSerializer;
        }

        private static string TypeNameWithoutGenericEnding(TypeReference type)
        {
            var typeName = type.Name;

            // Remove generics ending (i.e. `1)
            var genericCharIndex = typeName.LastIndexOf('`');
            if (genericCharIndex != -1)
                typeName = typeName.Substring(0, genericCharIndex);

            return typeName;
        }

        public static string SerializerTypeName(TypeReference type, bool appendGenerics, bool appendSerializer)
        {
            var typeName = TypeNameWithoutGenericEnding(type);

            // Prepend nested class
            if (type.IsNested)
                typeName = TypeNameWithoutGenericEnding(type.DeclaringType) + "_" + typeName;

            // Prepend namespace
            if (!String.IsNullOrEmpty(type.Namespace))
                typeName = type.Namespace.Replace(".", String.Empty) + "_" + typeName;

            // Append Serializer
            if (appendSerializer)
                typeName += "Serializer";

            // Append Generics
            if (appendGenerics)
                typeName += type.GenerateGenerics();

            return typeName;
        }

        public static string GetSerializerInstantiateMethodName(TypeReference serializerType, bool appendGenerics)
        {
            return "Instantiate_" + SerializerTypeName(serializerType, appendGenerics, false);
        }

        /// <summary>
        /// Generates the generic constraints in a code form.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GenerateGenericConstraints(TypeReference type)
        {
            if (!type.HasGenericParameters)
                return String.Empty;

            var result = new StringBuilder();
            foreach (var genericParameter in type.GenericParameters)
            {
                // If no constraints, skip it
                var hasContraints = genericParameter.HasReferenceTypeConstraint || genericParameter.HasNotNullableValueTypeConstraint || genericParameter.Constraints.Count > 0 || genericParameter.HasDefaultConstructorConstraint;
                if (!hasContraints)
                {
                    continue;
                }

                bool hasFirstContraint = false;

                result.AppendFormat(" where {0}: ", genericParameter.Name);

                // Where class/struct constraint must be before any other constraint
                if (genericParameter.HasReferenceTypeConstraint)
                {
                    result.AppendFormat("class");
                    hasFirstContraint = true;
                }
                else if (genericParameter.HasNotNullableValueTypeConstraint)
                {
                    result.AppendFormat("struct");
                    hasFirstContraint = true;
                }

                foreach (var genericParameterConstraint in genericParameter.Constraints)
                {
                    // Skip value type constraint
                    if (genericParameterConstraint.FullName != typeof(ValueType).FullName)
                    {
                        if (hasFirstContraint)
                        {
                            result.Append(", ");
                        }

                        result.AppendFormat("{0}", genericParameterConstraint.ConvertCSharp());
                        result.AppendLine();

                        hasFirstContraint = true;
                    }
                }


                // New constraint must be last
                if (!genericParameter.HasNotNullableValueTypeConstraint && genericParameter.HasDefaultConstructorConstraint)
                {
                    if (hasFirstContraint)
                    {
                        result.Append(", ");
                    }

                    result.AppendFormat("new()");
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Determines whether the specified type has an empty constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected static bool HasEmptyConstructor(TypeReference type)
        {
            return type.Resolve().Methods.Any(x => x.IsConstructor && x.IsPublic && !x.IsStatic && x.Parameters.Count == 0);
        }

        public static void IgnoreMember(IMemberDefinition memberInfo)
        {
            ignoredMembers.Add(memberInfo);
        }

        public static IEnumerable<SerializableItem> GetSerializableItems(TypeReference type, bool serializeFields, ComplexTypeSerializerFlags? flagsOverride = null)
        {
            foreach (var serializableItemOriginal in GetSerializableItems(type.Resolve(), serializeFields, flagsOverride))
            {
                var serializableItem = serializableItemOriginal;

                // Try to resolve open generic types with context to have closed types.
                if (serializableItem.Type.ContainsGenericParameter())
                {
                    serializableItem.Type = ResolveGenericsVisitor.Process(type, serializableItem.Type);
                }

                yield return serializableItem;
            }
        }

        public static IEnumerable<SerializableItem> GetSerializableItems(TypeDefinition type, bool serializeFields, ComplexTypeSerializerFlags? flagsOverride = null)
        {
            ComplexTypeSerializerFlags flags;

            var fields = new List<FieldDefinition>();
            var properties = new List<PropertyDefinition>();

            var fieldEnum = type.Fields.Where(x => (x.IsPublic || (x.IsAssembly && x.CustomAttributes.Any(a => a.AttributeType.FullName == "SiliconStudio.Core.DataMemberAttribute"))) && !x.IsStatic && !ignoredMembers.Contains(x));

            // If there is a explicit or sequential layout, use offset, otherwise use name
            // (not sure if Cecil follow declaration order, in which case it could be OK to not sort;
            // sorting has the advantage of being more resistant to type upgrade, when field is added/remove, as long as field name is saved)
            if (type.IsSequentialLayout || type.IsExplicitLayout)
                fieldEnum = fieldEnum.OrderBy(x => x.Offset);
            else
                fieldEnum = fieldEnum.OrderBy(x => x.Name);

            foreach (var field in fieldEnum)
            {
                fields.Add(field);
            }

            foreach (var property in type.Properties.OrderBy(x => x.Name))
            {
                // Need a non-static public get method
                if (property.GetMethod == null || !property.GetMethod.IsPublic || property.GetMethod.IsStatic)
                    continue;

                // If it's a struct (!IsValueType), we need a public set method as well
                if (property.PropertyType.IsValueType && (property.SetMethod == null || !(property.SetMethod.IsAssembly || property.SetMethod.IsPublic)))
                    continue;

                // Only take virtual properties (override ones will be handled by parent serializers)
                if (property.GetMethod.IsVirtual && !property.GetMethod.IsNewSlot)
                {
                    // Exception: if this one has a DataMember, let's assume parent one was Ignore and we explicitly want to serialize this one
                    if (!property.CustomAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.DataMemberAttribute"))
                        continue;
                }

                // Ignore blacklisted properties
                if (ignoredMembers.Contains(property))
                    continue;

                properties.Add(property);
            }

            if (flagsOverride.HasValue)
                flags = flagsOverride.Value;
            else if (type.IsClass && !type.IsValueType)
                flags = ComplexTypeSerializerFlags.SerializePublicFields | ComplexTypeSerializerFlags.SerializePublicProperties;
            else if (type.Fields.Any(x => x.IsPublic && !x.IsStatic))
                flags = ComplexTypeSerializerFlags.SerializePublicFields;
            else
                flags = ComplexTypeSerializerFlags.SerializePublicProperties;

            if ((flags & ComplexTypeSerializerFlags.SerializePublicFields) != 0)
            {
                foreach (var field in fields)
                {
                    if (IsMemberIgnored(field.CustomAttributes, flags)) continue;
                    var attributes = field.CustomAttributes;
                    var fixedAttribute = field.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(FixedBufferAttribute).FullName);
                    var assignBack = !field.IsInitOnly;

                    // If not assigned back, check that type is serializable in place
                    if (!assignBack && !IsReadOnlyTypeSerializable(field.FieldType))
                        continue;

                    yield return new SerializableItem { MemberInfo = field, Type = field.FieldType, Name = field.Name, Attributes = attributes, AssignBack = assignBack, NeedReference = false, HasFixedAttribute = fixedAttribute != null };
                }
            }
            if ((flags & ComplexTypeSerializerFlags.SerializePublicProperties) != 0)
            {
                // Only process properties with public get and set methods
                foreach (var property in properties)
                {
                    // Ignore properties with indexer
                    if (property.GetMethod.Parameters.Count > 0)
                        continue;
                    if (IsMemberIgnored(property.CustomAttributes, flags)) continue;
                    var attributes = property.CustomAttributes;
                    var assignBack = property.SetMethod != null && (property.SetMethod.IsPublic || property.SetMethod.IsAssembly);

                    // If not assigned back, check that type is serializable in place
                    if (!assignBack && !IsReadOnlyTypeSerializable(property.PropertyType))
                        continue;

                    yield return new SerializableItem { MemberInfo = property, Type = property.PropertyType, Name = property.Name, Attributes = attributes, AssignBack = assignBack, NeedReference = !type.IsClass || type.IsValueType };
                }
            }
        }

        private static bool IsMemberIgnored(ICollection<CustomAttribute> customAttributes, ComplexTypeSerializerFlags flags)
        {
            // Check for DataMemberIgnore
            if (customAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.DataMemberIgnoreAttribute"))
            {
                // Still allow members with DataMemberUpdatable if we are running UpdateEngineProcessor
                if (!((flags & ComplexTypeSerializerFlags.Updatable) != 0
                      && customAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Xenko.Updater.DataMemberUpdatableAttribute")))
                    return true;
            }
            return false;
        }

        private static bool IsReadOnlyTypeSerializable(TypeReference type)
        {
            // For now, we allow any class which is not a string (since they are immutable)
            return type.MetadataType != MetadataType.String && type.Resolve().IsClass;
        }

        protected static string CreateMemberVariableName(IMemberDefinition memberInfo)
        {
            var memberVariableName = Char.ToLowerInvariant(memberInfo.Name[0]) + memberInfo.Name.Substring(1);
            if (forbiddenKeywords.Contains(memberVariableName))
                memberVariableName += "_";
            return memberVariableName;
        }

        protected IEnumerable<TypeReference> EnumerateSerializerTypes(IEnumerable<TypeReference> memberTypes)
        {
            var result = new HashSet<TypeReference>();
            var objectTypes = new HashSet<string>();
            foreach (var memberType in memberTypes)
            {
                EnumerateSerializerTypes(memberType, objectTypes, result);
            }
            return result;
        }

        protected void EnumerateSerializerTypes(TypeReference objectType, HashSet<string> objectTypes, HashSet<TypeReference> serializerTypes)
        {
            if (objectType.IsGenericParameter)
                return;

            // Already processed?
            if (!objectTypes.Add(objectType.FullName))
                return;

            //foreach (var serializerFactory in serializerFactories)
            //{
            //    var serializerType = serializerFactory.GetSerializer(objectType);
            //
            //    // Did we find a new serializer type?
            //    if (serializerType == null || !serializerTypes.Add(serializerType))
            //        continue;
            //
            //    // If yes, recurse on object types this serializer might require, so that they can in turn ask for their serializer
            //    // It is useful for cases such as List<List<A>>.
            //    foreach (var serializerDependency in serializerDependencies)
            //    {
            //        var subObjectTypes = serializerDependency.EnumerateSubTypesFromSerializer(serializerType);
            //
            //        // Could be null (to avoid unecessary empty enumerables)
            //        if (subObjectTypes == null)
            //            continue;
            //
            //        foreach (var subObjectType in subObjectTypes)
            //        {
            //            EnumerateSerializerTypes(subObjectType, objectTypes, serializerTypes);
            //        }
            //    }
            //}
        }


        public struct SerializableItem
        {
            public bool HasFixedAttribute;
            public string Name;
            public IMemberDefinition MemberInfo;
            public TypeReference Type { get; set; }
            public bool NeedReference;
            public bool AssignBack;
            public IList<CustomAttribute> Attributes;
        }
    }
}
#endif