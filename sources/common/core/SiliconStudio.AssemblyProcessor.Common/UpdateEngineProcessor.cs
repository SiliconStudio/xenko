// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.AssemblyProcessor.Serializers;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace SiliconStudio.AssemblyProcessor
{
    internal partial class UpdateEngineProcessor : ICecilSerializerProcessor
    {
        private ModuleDefinition siliconStudioCoreModule;

        private TypeDefinition updatableFieldGenericType;
        private MethodDefinition updatableFieldGenericCtor;

        private TypeDefinition updatablePropertyGenericType;
        private MethodDefinition updatablePropertyGenericCtor;

        private MethodReference updateEngineRegisterMemberMethod;

        private MethodReference getTypeFromHandleMethod;

        public void ProcessSerializers(CecilSerializerContext context)
        {
            // Generate IL for SiliconStudio.Core
            if (context.Assembly.Name.Name == "SiliconStudio.Core")
            {
                ProcessCoreAssembly(context);
            }

            // Get or create method
            var updateEngineType = GetOrCreateUpdateType(context.Assembly, true);
            var mainPrepareMethod = new MethodDefinition("UpdateMain", MethodAttributes.HideBySig | MethodAttributes.Assembly | MethodAttributes.Static, context.Assembly.MainModule.TypeSystem.Void);
            updateEngineType.Methods.Add(mainPrepareMethod);

            // Get some useful Cecil objects from SiliconStudio.Core
            var siliconStudioCoreAssembly = context.Assembly.Name.Name == "SiliconStudio.Core"
                    ? context.Assembly
                    : context.Assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Core");
            siliconStudioCoreModule = siliconStudioCoreAssembly.MainModule;

            updatableFieldGenericType = siliconStudioCoreModule.GetType("SiliconStudio.Core.Updater.UpdatableField`1");
            updatableFieldGenericCtor = updatableFieldGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            updatablePropertyGenericType = siliconStudioCoreModule.GetType("SiliconStudio.Core.Updater.UpdatableProperty`1");
            updatablePropertyGenericCtor = updatablePropertyGenericType.Methods.First(x => x.IsConstructor && !x.IsStatic);

            updateEngineRegisterMemberMethod = context.Assembly.MainModule.ImportReference(siliconStudioCoreModule.GetType("SiliconStudio.Core.Updater.UpdateEngine").Methods.First(x => x.Name == "RegisterMember"));

            var typeType = CecilExtensions.FindCorlibAssembly(context.Assembly).MainModule.GetTypeResolved(typeof(Type).FullName);
            getTypeFromHandleMethod = context.Assembly.MainModule.Import(typeType.Methods.First(x => x.Name == "GetTypeFromHandle"));

            // Make sure it is called at module startup
            var moduleInitializerAttribute = siliconStudioCoreModule.GetType("SiliconStudio.Core.ModuleInitializerAttribute");
            mainPrepareMethod.CustomAttributes.Add(new CustomAttribute(context.Assembly.MainModule.ImportReference(moduleInitializerAttribute.GetConstructors().Single(x => !x.IsStatic))));

            // Emit serialization code for all the types we care about
            foreach (var serializableType in context.ComplexTypes.Select(x => x.Key))
            {
                try
                {
                    ProcessType(context, serializableType, mainPrepareMethod);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Error when generating update engine code for {0}", serializableType), e);
                }
            }

            // Force generic instantiations
            var il = mainPrepareMethod.Body.GetILProcessor();
            foreach (var serializableType in context.SerializableTypesProfiles.SelectMany(x => x.Value.SerializableTypes.Where(y => y.Value.Local)).Select(x => x.Key).Distinct().OfType<GenericInstanceType>())
            {
                // Try to find if original method definition was generated
                var typeDefinition = serializableType.Resolve();

                var updateMethod = GetOrCreateUpdateType(typeDefinition.Module.Assembly, false)?.Methods.FirstOrDefault(x => x.Name == ComputeUpdateMethodName(typeDefinition));

                // If nothing was found in main assembly, also look in SiliconStudio.Core assembly, just in case (it might defines some shared/corlib types -- currently not the case)
                if (updateMethod == null)
                {
                    var coreSerializationAssembly = context.Assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Core");
                    if (coreSerializationAssembly != null)
                    {
                        updateMethod = GetOrCreateUpdateType(coreSerializationAssembly, false)?.Methods.FirstOrDefault(x => x.Name == ComputeUpdateMethodName(typeDefinition));
                    }
                }

                if (updateMethod == null)
                    continue;

                // Emit call to update engine setup method with generic arguments of current type
                il.Emit(OpCodes.Call, context.Assembly.MainModule.ImportReference(updateMethod)
                    .MakeGenericMethod(serializableType.GenericArguments
                        .Select(context.Assembly.MainModule.ImportReference)
                        .Select(CecilExtensions.FixupValueType).ToArray()));
            }

            il.Emit(OpCodes.Ret);
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

            foreach (var field in typeDefinition.Fields)
            {
                // Only public non-static non-generic fields
                if (field.IsStatic || !field.IsPublic)
                    continue;

                var fieldReference = context.Assembly.MainModule.ImportReference(field.MakeGeneric(updateCurrentMethod.GenericParameters.ToArray()));

                // First time it is needed, let's create empty object: var emptyObject = new object();
                if (emptyObjectField == null)
                {
                    emptyObjectField = new FieldDefinition("emptyObject", FieldAttributes.Static | FieldAttributes.Private, context.Assembly.MainModule.TypeSystem.Object);

                    // Create static ctor that will initialize this object
                    var staticConstructor = new MethodDefinition(".cctor",
                                            MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
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
                il.Emit(OpCodes.Ldflda, fieldReference);
                il.Emit(OpCodes.Ldsfld, emptyObjectField);
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Conv_I4);

                var fieldType = replaceGenericsVisitor != null ? replaceGenericsVisitor.VisitDynamic(field.FieldType) : field.FieldType;
                il.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(updatableFieldGenericCtor).MakeGeneric(context.Assembly.MainModule.ImportReference(fieldType)));
                il.Emit(OpCodes.Call, updateEngineRegisterMemberMethod);
            }

            foreach (var property in typeDefinition.Properties)
            {
                // Only non-static properties with public accessor and no indexers
                if (property.GetMethod == null || !property.GetMethod.IsPublic || property.GetMethod.IsStatic || property.HasParameters)
                    continue;

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

                var propertyType = replaceGenericsVisitor != null ? replaceGenericsVisitor.VisitDynamic(property.PropertyType) : property.PropertyType;
                il.Emit(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(updatablePropertyGenericCtor).MakeGeneric(context.Assembly.MainModule.ImportReference(propertyType)));
                il.Emit(OpCodes.Call, updateEngineRegisterMemberMethod);
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
            var typeName = typeDefinition.FullName.Replace(".", "_");
            var typeNameGenericPart = typeName.IndexOf("`");
            if (typeNameGenericPart != -1)
                typeName = typeName.Substring(0, typeNameGenericPart);

            return string.Format("UpdateGeneric_{0}", typeName);
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
    }
};