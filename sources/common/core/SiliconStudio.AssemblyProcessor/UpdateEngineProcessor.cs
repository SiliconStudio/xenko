// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.AssemblyProcessor.Serializers;
using SiliconStudio.Core.Serialization;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace SiliconStudio.AssemblyProcessor
{
    internal partial class UpdateEngineProcessor : ICecilSerializerProcessor
    {
        private MethodDefinition updatableFieldGenericCtor;
        private MethodDefinition updatableListUpdateResolverGenericCtor;
        private MethodDefinition updatableArrayUpdateResolverGenericCtor;

        private TypeDefinition updatablePropertyGenericType;
        private MethodDefinition updatablePropertyGenericCtor;
        private MethodDefinition updatablePropertyObjectGenericCtor;

        private MethodReference updateEngineRegisterMemberMethod;
        private MethodReference updateEngineRegisterMemberResolverMethod;

        private MethodReference getTypeFromHandleMethod;

        private TypeDefinition animationDataType;

        public void ProcessSerializers(CecilSerializerContext context)
        {
            var references = new HashSet<AssemblyDefinition>();
            EnumerateReferences(references, context.Assembly);

            var coreAssembly = CecilExtensions.FindCorlibAssembly(context.Assembly);

            // Only process assemblies depending on Xenko.Engine
            if (!references.Any(x => x.Name.Name == "SiliconStudio.Xenko.Engine"))
            {
                // Make sure Xenko.Engine.Serializers can access everything internally
                var internalsVisibleToAttribute = coreAssembly.MainModule.GetTypeResolved(typeof(InternalsVisibleToAttribute).FullName);
                var serializationAssemblyName = "SiliconStudio.Xenko.Engine.Serializers";

                // Add [InteralsVisibleTo] attribute
                var internalsVisibleToAttributeCtor = context.Assembly.MainModule.ImportReference(internalsVisibleToAttribute.GetConstructors().Single());
                var internalsVisibleAttribute = new CustomAttribute(internalsVisibleToAttributeCtor)
                {
                    ConstructorArguments =
                            {
                                new CustomAttributeArgument(context.Assembly.MainModule.ImportReference(context.Assembly.MainModule.TypeSystem.String), serializationAssemblyName)
                            }
                };
                context.Assembly.CustomAttributes.Add(internalsVisibleAttribute);

                return;
            }

            // Get or create method
            var updateEngineType = GetOrCreateUpdateType(context.Assembly, true);
            var mainPrepareMethod = new MethodDefinition("UpdateMain", MethodAttributes.HideBySig | MethodAttributes.Assembly | MethodAttributes.Static, context.Assembly.MainModule.TypeSystem.Void);
            updateEngineType.Methods.Add(mainPrepareMethod);

            // Get some useful Cecil objects from SiliconStudio.Core
            var siliconStudioCoreAssembly = context.Assembly.Name.Name == "SiliconStudio.Core"
                    ? context.Assembly
                    : context.Assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Core");
            var siliconStudioCoreModule = siliconStudioCoreAssembly.MainModule;

            var siliconStudioXenkoEngineAssembly = context.Assembly.Name.Name == "SiliconStudio.Xenko.Engine"
                    ? context.Assembly
                    : context.Assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Xenko.Engine");
            var siliconStudioXenkoEngineModule = siliconStudioXenkoEngineAssembly.MainModule;

            // Generate IL for SiliconStudio.Core
            if (context.Assembly.Name.Name == "SiliconStudio.Xenko.Engine")
            {
                ProcessXenkoEngineAssembly(context);
            }
            else
            {
#if true || SILICONSTUDIO_XENKO_XAMARIN_CALLI_BUG
                // We still process UpdatableProperty<T> since we had to revert it back when writing back Xenko.Engine (otherwise it crashes at AOT on iOS)
                new UpdatablePropertyCodeGenerator(siliconStudioXenkoEngineAssembly).GenerateUpdatablePropertyCode();
#endif
            }

            animationDataType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Animations.AnimationData`1");

            var updatableFieldGenericType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.UpdatableField`1");
            updatableFieldGenericCtor = updatableFieldGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            updatablePropertyGenericType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.UpdatableProperty`1");
            updatablePropertyGenericCtor = updatablePropertyGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            var updatablePropertyObjectGenericType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.UpdatablePropertyObject`1");
            updatablePropertyObjectGenericCtor = updatablePropertyObjectGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            var updatableListUpdateResolverGenericType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.ListUpdateResolver`1");
            updatableListUpdateResolverGenericCtor = updatableListUpdateResolverGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            var updatableArrayUpdateResolverGenericType = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.ArrayUpdateResolver`1");
            updatableArrayUpdateResolverGenericCtor = updatableArrayUpdateResolverGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            var registerMemberMethod = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.UpdateEngine").Methods.First(x => x.Name == "RegisterMember");
            var pclVisitor = new PclFixupTypeVisitor(coreAssembly);
            pclVisitor.VisitMethod(registerMemberMethod);
            updateEngineRegisterMemberMethod = context.Assembly.MainModule.ImportReference(registerMemberMethod);

            var registerMemberResolverMethod = siliconStudioXenkoEngineModule.GetType("SiliconStudio.Xenko.Updater.UpdateEngine") .Methods.First(x => x.Name == "RegisterMemberResolver");
            pclVisitor.VisitMethod(registerMemberResolverMethod);
            updateEngineRegisterMemberResolverMethod = context.Assembly.MainModule.ImportReference(registerMemberResolverMethod);

            var typeType = coreAssembly.MainModule.GetTypeResolved(typeof(Type).FullName);
            getTypeFromHandleMethod = context.Assembly.MainModule.ImportReference(typeType.Methods.First(x => x.Name == "GetTypeFromHandle"));

            // Make sure it is called at module startup
            var moduleInitializerAttribute = siliconStudioCoreModule.GetType("SiliconStudio.Core.ModuleInitializerAttribute");
            var ctorMethod = moduleInitializerAttribute.GetConstructors().Single(x => !x.IsStatic);
            pclVisitor.VisitMethod(ctorMethod);
            mainPrepareMethod.CustomAttributes.Add(new CustomAttribute(context.Assembly.MainModule.ImportReference(ctorMethod)));

            // Emit serialization code for all the types we care about
            var processedTypes = new HashSet<TypeDefinition>(TypeReferenceEqualityComparer.Default);
            foreach (var serializableType in context.SerializableTypesProfiles.SelectMany(x => x.Value.SerializableTypes))
            {
                // Special case: when processing Xenko.Engine assembly, we automatically add dependent assemblies types too
                if (!serializableType.Value.Local && siliconStudioXenkoEngineAssembly != context.Assembly)
                    continue;

                var typeDefinition = serializableType.Key as TypeDefinition;
                if (typeDefinition == null)
                    continue;

                // Ignore already processed types
                if (!processedTypes.Add(typeDefinition))
                    continue;

                try
                {
                    ProcessType(context, typeDefinition, mainPrepareMethod);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Error when generating update engine code for {0}", typeDefinition), e);
                }
            }

            // Force generic instantiations
            var il = mainPrepareMethod.Body.GetILProcessor();
            foreach (var serializableType in context.SerializableTypesProfiles.SelectMany(x => x.Value.SerializableTypes).ToArray())
            {
                // Special case: when processing Xenko.Engine assembly, we automatically add dependent assemblies types too
                if (!serializableType.Value.Local && siliconStudioXenkoEngineAssembly != context.Assembly)
                    continue;

                // Make sure AnimationData<T> is serializable
                //if (serializableType.Value.Mode == DataSerializerGenericMode.None)
                //    context.GenerateSerializer(context.Assembly.MainModule.ImportReference(animationDataType).MakeGenericType(context.Assembly.MainModule.ImportReference(serializableType.Key)));

                // Try to find if original method definition was generated
                var typeDefinition = serializableType.Key.Resolve();

                // If using List<T>, register this type in UpdateEngine
                var listInterfaceType = typeDefinition.Interfaces.OfType<GenericInstanceType>().FirstOrDefault(x => x.ElementType.FullName == typeof(IList<>).FullName);
                if (listInterfaceType != null)
                {
                    //call Updater.UpdateEngine.RegisterMemberResolver(new Updater.ListUpdateResolver<T>());
                    var elementType = ResolveGenericsVisitor.Process(serializableType.Key, listInterfaceType.GenericArguments[0]);
                    il.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(updatableListUpdateResolverGenericCtor).MakeGeneric(context.Assembly.MainModule.ImportReference(elementType).FixupValueType()));
                    il.Emit(OpCodes.Call, updateEngineRegisterMemberResolverMethod);
                }

                // Same for arrays
                var arrayType = serializableType.Key as ArrayType;
                if (arrayType != null)
                {
                    //call Updater.UpdateEngine.RegisterMemberResolver(new Updater.ArrayUpdateResolver<T>());
                    var elementType = ResolveGenericsVisitor.Process(serializableType.Key, arrayType.ElementType);
                    il.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(updatableArrayUpdateResolverGenericCtor).MakeGeneric(context.Assembly.MainModule.ImportReference(elementType).FixupValueType()));
                    il.Emit(OpCodes.Call, updateEngineRegisterMemberResolverMethod);
                }

                var genericInstanceType = serializableType.Key as GenericInstanceType;
                if (genericInstanceType != null)
                {
                    var expectedUpdateMethodName = ComputeUpdateMethodName(typeDefinition);
                    var updateMethod = GetOrCreateUpdateType(typeDefinition.Module.Assembly, false)?.Methods.FirstOrDefault(x => x.Name == expectedUpdateMethodName && x.HasGenericParameters && x.GenericParameters.Count == genericInstanceType.GenericParameters.Count);

                    // If nothing was found in main assembly, also look in SiliconStudio.Xenko.Engine assembly, just in case (it might defines some shared/corlib types -- currently not the case)
                    if (updateMethod == null)
                    {
                        updateMethod = GetOrCreateUpdateType(siliconStudioXenkoEngineAssembly, false)?.Methods.FirstOrDefault(x => x.Name == expectedUpdateMethodName && x.HasGenericParameters && x.GenericParameters.Count == genericInstanceType.GenericParameters.Count);
                    }

                    if (updateMethod != null)
                    {
                        // Emit call to update engine setup method with generic arguments of current type
                        il.Emit(OpCodes.Call, context.Assembly.MainModule.ImportReference(updateMethod)
                            .MakeGenericMethod(genericInstanceType.GenericArguments
                                .Select(context.Assembly.MainModule.ImportReference)
                                .Select(CecilExtensions.FixupValueType).ToArray()));
                    }
                }
            }

            il.Emit(OpCodes.Ret);

#if true || SILICONSTUDIO_XENKO_XAMARIN_CALLI_BUG
            // Due to Xamarin iOS AOT limitation, we can't keep this type around because it fails compilation
            if (context.Assembly.Name.Name == "SiliconStudio.Xenko.Engine")
            {
                NotImplementedBody(updatablePropertyGenericType.Methods.First(x => x.Name == "GetStructAndUnbox"));
                NotImplementedBody(updatablePropertyGenericType.Methods.First(x => x.Name == "GetBlittable"));
                NotImplementedBody(updatablePropertyGenericType.Methods.First(x => x.Name == "SetStruct"));
                NotImplementedBody(updatablePropertyGenericType.Methods.First(x => x.Name == "SetBlittable"));
            }
#endif
        }

        public void ProcessType(CecilSerializerContext context, TypeReference type, MethodDefinition updateMainMethod)
        {
            var typeDefinition = type.Resolve();

            // No need to process enum
            if (typeDefinition.IsEnum)
                return;

            var updateCurrentMethod = updateMainMethod;
            ResolveGenericsVisitor replaceGenericsVisitor = null;

            if (typeDefinition.HasGenericParameters)
            {
                // Make a prepare method for just this object since it might need multiple instantiation
                updateCurrentMethod = new MethodDefinition(ComputeUpdateMethodName(typeDefinition), MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static, context.Assembly.MainModule.TypeSystem.Void);
                var genericsMapping = new Dictionary<TypeReference, TypeReference>();
                foreach (var genericParameter in typeDefinition.GenericParameters)
                {
                    var genericParameterCopy = new GenericParameter(genericParameter.Name, updateCurrentMethod)
                    {
                        Attributes = genericParameter.Attributes,
                    };
                    foreach (var constraint in genericParameter.Constraints)
                        genericParameterCopy.Constraints.Add(context.Assembly.MainModule.ImportReference(constraint));
                    updateCurrentMethod.GenericParameters.Add(genericParameterCopy);

                    genericsMapping[genericParameter] = genericParameterCopy;
                }

                replaceGenericsVisitor = new ResolveGenericsVisitor(genericsMapping);

                updateMainMethod.DeclaringType.Methods.Add(updateCurrentMethod);
            }

            var il = updateCurrentMethod.Body.GetILProcessor();
            var emptyObjectField = updateMainMethod.DeclaringType.Fields.FirstOrDefault(x => x.Name == "emptyObject");

            // Note: forcing fields and properties to be processed in all cases
            foreach (var serializableItem in ComplexClassSerializerGenerator.GetSerializableItems(type, true, ComplexTypeSerializerFlags.SerializePublicFields | ComplexTypeSerializerFlags.SerializePublicProperties | ComplexTypeSerializerFlags.Updatable))
            {
                var fieldReference = serializableItem.MemberInfo as FieldReference;
                if (fieldReference != null)
                {
                    var field = fieldReference.Resolve();

                    // First time it is needed, let's create empty object: var emptyObject = new object();
                    if (emptyObjectField == null)
                    {
                        emptyObjectField = new FieldDefinition("emptyObject", FieldAttributes.Static | FieldAttributes.Private, context.Assembly.MainModule.TypeSystem.Object);

                        // Create static ctor that will initialize this object
                        var staticConstructor = new MethodDefinition(".cctor",
                                                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                context.Assembly.MainModule.TypeSystem.Void);
                        var staticConstructorIL = staticConstructor.Body.GetILProcessor();
                        staticConstructorIL.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(emptyObjectField.FieldType.Resolve().GetConstructors().Single(x => !x.IsStatic && !x.HasParameters)));
                        staticConstructorIL.Emit(OpCodes.Stsfld, emptyObjectField);
                        staticConstructorIL.Emit(OpCodes.Ret);

                        updateMainMethod.DeclaringType.Fields.Add(emptyObjectField);
                        updateMainMethod.DeclaringType.Methods.Add(staticConstructor);
                    }

                    il.Emit(OpCodes.Ldtoken, type);
                    il.Emit(OpCodes.Call, getTypeFromHandleMethod);
                    il.Emit(OpCodes.Ldstr, field.Name);

                    il.Emit(OpCodes.Ldsfld, emptyObjectField);
                    il.Emit(OpCodes.Ldflda, context.Assembly.MainModule.ImportReference(fieldReference));
                    il.Emit(OpCodes.Conv_I);
                    il.Emit(OpCodes.Ldsfld, emptyObjectField);
                    il.Emit(OpCodes.Conv_I);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Conv_I4);

                    var fieldType = context.Assembly.MainModule.ImportReference(replaceGenericsVisitor != null ? replaceGenericsVisitor.VisitDynamic(field.FieldType) : field.FieldType).FixupValueType();
                    il.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(updatableFieldGenericCtor).MakeGeneric(fieldType));
                    il.Emit(OpCodes.Call, updateEngineRegisterMemberMethod);
                }

                var propertyReference = serializableItem.MemberInfo as PropertyReference;
                if (propertyReference != null)
                {
                    var property = propertyReference.Resolve();

                    var propertyGetMethod = context.Assembly.MainModule.ImportReference(property.GetMethod).MakeGeneric(updateCurrentMethod.GenericParameters.ToArray());

                    il.Emit(OpCodes.Ldtoken, type);
                    il.Emit(OpCodes.Call, getTypeFromHandleMethod);
                    il.Emit(OpCodes.Ldstr, property.Name);

                    il.Emit(OpCodes.Ldftn, propertyGetMethod);

                    // Only get setter if it exists and it's public
                    if (property.SetMethod != null && property.SetMethod.IsPublic)
                    {
                        var propertySetMethod = context.Assembly.MainModule.ImportReference(property.SetMethod).MakeGeneric(updateCurrentMethod.GenericParameters.ToArray());
                        il.Emit(OpCodes.Ldftn, propertySetMethod);
                    }
                    else
                    {
                        // 0 (native int)
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Conv_I);
                    }

                    var propertyType = context.Assembly.MainModule.ImportReference(replaceGenericsVisitor != null ? replaceGenericsVisitor.VisitDynamic(property.PropertyType) : property.PropertyType).FixupValueType();

                    var updatablePropertyInflatedCtor = GetOrCreateUpdatablePropertyCtor(context.Assembly, propertyType);

                    il.Emit(OpCodes.Newobj, updatablePropertyInflatedCtor);
                    il.Emit(OpCodes.Call, updateEngineRegisterMemberMethod);
                }
            }

            if (updateCurrentMethod != updateMainMethod)
            {
                // If we have a local method, close it
                il.Emit(OpCodes.Ret);

                // Also call it from main method if it was a closed generic instantiation
                if (type is GenericInstanceType)
                {
                    il = updateMainMethod.Body.GetILProcessor();
                    il.Emit(OpCodes.Call, updateCurrentMethod.MakeGeneric(((GenericInstanceType)type).GenericArguments.Select(context.Assembly.MainModule.ImportReference).Select(CecilExtensions.FixupValueType).ToArray()));
                }
            }
        }


        private static string ComputeUpdateMethodName(TypeDefinition typeDefinition)
        {
            var typeName = ComputeTypeName(typeDefinition);

            return string.Format("UpdateGeneric_{0}", typeName);
        }

        private static string ComputeTypeName(TypeDefinition typeDefinition)
        {
            var typeName = typeDefinition.FullName.Replace(".", "_");
            var typeNameGenericPart = typeName.IndexOf("`");
            if (typeNameGenericPart != -1)
                typeName = typeName.Substring(0, typeNameGenericPart);
            return typeName;
        }

        private static TypeDefinition GetOrCreateUpdateType(AssemblyDefinition assembly, bool createIfNotExists)
        {
            // Get or create module static constructor
            var updateEngineType = assembly.MainModule.Types.FirstOrDefault(x => x.Name == "UpdateEngineAutoGenerated");
            if (updateEngineType == null && createIfNotExists)
            {
                updateEngineType = new TypeDefinition(string.Empty, "UpdateEngineAutoGenerated", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
                updateEngineType.BaseType = assembly.MainModule.TypeSystem.Object;
                assembly.MainModule.Types.Add(updateEngineType);
            }

            return updateEngineType;
        }

        public MethodReference GetOrCreateUpdatablePropertyCtor(AssemblyDefinition assembly, TypeReference propertyType)
        {
            if (propertyType.Resolve().IsValueType)
            {
#if true || SILICONSTUDIO_XENKO_XAMARIN_CALLI_BUG
                // Temporary code until Xamarin fixes bugs with generics calli
                // For now, we simply create one class per type T instead of using UpdatableProperty<T>
                // Later, we should only use UpdatableProperty<T> for everything
                var updateEngineType = GetOrCreateUpdateType(assembly, true);
                var updatablePropertyInflatedTypeName = $"UpdatableProperty_{propertyType.ConvertCSharp().Replace('.', '_').Replace("[]", "Array")}";
                var updatablePropertyInflatedType = updateEngineType.NestedTypes.FirstOrDefault(x => x.Name == updatablePropertyInflatedTypeName);
                if (updatablePropertyInflatedType == null)
                {
                    var genericMapping = new Dictionary<TypeReference, TypeReference>();
                    genericMapping.Add(updatablePropertyGenericType.GenericParameters[0], propertyType);

                    updatablePropertyInflatedType = new TypeDefinition(string.Empty, updatablePropertyInflatedTypeName, TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
                    updateEngineType.NestedTypes.Add(updatablePropertyInflatedType);

                    CecilExtensions.InflateGenericType(updatablePropertyGenericType, updatablePropertyInflatedType, propertyType);
                }

                var updatablePropertyCtor = updatablePropertyInflatedType.Methods.First(x => x.IsConstructor && !x.IsStatic);

                return assembly.MainModule.ImportReference(updatablePropertyCtor);
#else
                return assembly.MainModule.ImportReference(updatablePropertyGenericCtor).MakeGeneric(propertyType);
#endif
            }
            else
            {
                return assembly.MainModule.ImportReference(updatablePropertyObjectGenericCtor).MakeGeneric(propertyType);
            }
        }

        private static void EnumerateReferences(HashSet<AssemblyDefinition> assemblies, AssemblyDefinition assembly)
        {
            // Already processed?
            if (!assemblies.Add(assembly))
                return;

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
                    var referencedAssembly = assembly.MainModule.AssemblyResolver.Resolve(referencedAssemblyName);

                    EnumerateReferences(assemblies, referencedAssembly);
                }
                catch (AssemblyResolutionException)
                {
                }
            }
        }
    }
};
