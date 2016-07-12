// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace SiliconStudio.AssemblyProcessor
{
    internal class DispatcherProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            bool changed = false;
            foreach (var type in context.Assembly.EnumerateTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method.Body == null)
                        continue;

                    var ilProcessor = method.Body.GetILProcessor();
                    var instructions = method.Body.Instructions;

                    bool changes = false;

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var instruction = instructions[i];

                        if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference)
                        {
                            var methodDescription = (MethodReference)instruction.Operand;

                            if (methodDescription.Name == "ForEach" && methodDescription.DeclaringType.Name == "Dispatcher")
                            {
                                Debugger.Launch();

                                var delegateAllocation = instruction.Previous;
                                if (delegateAllocation.OpCode != OpCodes.Newobj)
                                {
                                    continue;
                                    throw new InvalidOperationException($"Expected NewObj instruction before before call to Dispatcher.ForEach. Found {delegateAllocation.OpCode}");
                                }

                                var functionPointerInstruction = delegateAllocation.Previous;

                                var loadClosureInstruction = functionPointerInstruction.Previous;

                                if (loadClosureInstruction.OpCode == OpCodes.Ldnull)
                                {
                                    // TODO: Make static
                                }
                                else
                                {
                                    int variableIndex = -1;
                                    OpCode storeOpCode;
                                    if (loadClosureInstruction.OpCode == OpCodes.Ldloc_0)
                                    {
                                        storeOpCode = OpCodes.Stloc_0;
                                    }
                                    else if (loadClosureInstruction.OpCode == OpCodes.Ldloc_1)
                                    {
                                        storeOpCode = OpCodes.Stloc_0;
                                    }
                                    else if (loadClosureInstruction.OpCode == OpCodes.Ldloc_2)
                                    {
                                        storeOpCode = OpCodes.Stloc_0;
                                    }
                                    else if (loadClosureInstruction.OpCode == OpCodes.Ldloc_3)
                                    {
                                        storeOpCode = OpCodes.Stloc_0;
                                    }
                                    else if (loadClosureInstruction.OpCode == OpCodes.Ldloc_S)
                                    {
                                        storeOpCode = OpCodes.Stloc_S;
                                        variableIndex = ((VariableReference)loadClosureInstruction.Operand).Index;
                                    }
                                    else if (loadClosureInstruction.OpCode == OpCodes.Ldloc)
                                    {
                                        storeOpCode = OpCodes.Stloc;
                                        variableIndex = ((VariableReference)loadClosureInstruction.Operand).Index;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("Could not find a closure load operation");
                                    }

                                    // Find closure allocation
                                    var storeClosureInstruction = loadClosureInstruction.Previous;
                                    while (storeClosureInstruction != null)
                                    {
                                        if (storeClosureInstruction.OpCode == storeOpCode && (variableIndex < 0 || variableIndex == ((VariableReference)storeClosureInstruction.Operand).Index))
                                            break;

                                        storeClosureInstruction = storeClosureInstruction.Previous;
                                    }

                                    if (storeClosureInstruction == null)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    var closureAllocation = storeClosureInstruction.Previous;

                                    if (closureAllocation.OpCode != OpCodes.Newobj)
                                    {
                                        // We might have processed it already
                                    }
                                    else
                                    {
                                        var closureConstructor = (MethodReference)closureAllocation.Operand;
                                        var closureType = closureConstructor.DeclaringType.Resolve();

                                        var mscorlibAssembly = context.Assembly.MainModule.AssemblyResolver.Resolve("mscorlib");

                                        var siliconStudioCoreAssembly = context.Assembly.Name.Name == "SiliconStudio.Core" ? context.Assembly :
                                            context.Assembly.MainModule.AssemblyResolver.Resolve("SiliconStudio.Core");

                                        var poolType = siliconStudioCoreAssembly.MainModule.GetType("SiliconStudio.Core.Threading.ConcurrentPool`1");

                                        // Create delegate field
                                        var delegateInstanceType = ((MethodReference)delegateAllocation.Operand).DeclaringType;
                                        var delegateFieldType = ChangeGenericArguments(context, delegateInstanceType, closureConstructor.DeclaringType);
                                        var delegateField = new FieldDefinition("<delegate>", FieldAttributes.Public, context.Assembly.MainModule.ImportReference(delegateFieldType));
                                        closureType.Fields.Add(delegateField);

                                        // Create method initializing new pool items
                                        var genericParameters = closureType.GenericParameters.Cast<TypeReference>().ToArray();
                                        var closureTypeConstructor = closureType.Methods.FirstOrDefault(x => x.Name == ".ctor");
                                        var closureInstanceType = closureType.HasGenericParameters ? (TypeReference)closureType.MakeGenericInstanceType(genericParameters) : closureType;
                                        var createPoolItemMethod = new MethodDefinition("<CreatePoolItem>", MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static, closureInstanceType);
                                        closureType.Methods.Add(createPoolItemMethod);
                                        createPoolItemMethod.Body.Variables.Add(new VariableDefinition(closureInstanceType));
                                        var ilProcessor3 = createPoolItemMethod.Body.GetILProcessor();
                                        // Create and store closure
                                        ilProcessor3.Emit(OpCodes.Newobj, closureType.HasGenericParameters ? closureTypeConstructor.MakeGeneric(genericParameters) : closureTypeConstructor);
                                        ilProcessor3.Emit(OpCodes.Stloc_0);
                                        ilProcessor3.Emit(OpCodes.Ldloc_0);
                                        // Create and set action
                                        ilProcessor3.Emit(OpCodes.Ldloc_0);
                                        //ilProcessor3.Emit(OpCodes.Ldftn, (MethodReference)functionPointerInstruction.Operand);
                                        //ilProcessor3.Emit(OpCodes.Newobj, (MethodReference)delegateAllocation.Operand);
                                        //// Store action
                                        ilProcessor3.Emit(OpCodes.Stfld, delegateField);
                                        //// Return closure
                                        ilProcessor3.Emit(OpCodes.Ldloc_0);
                                        ilProcessor3.Emit(OpCodes.Ret);

                                        // Create pool field
                                        var poolField = new FieldDefinition("<pool>", FieldAttributes.Public | FieldAttributes.Static, context.Assembly.MainModule.ImportReference(poolType).MakeGenericType(closureInstanceType));
                                        closureType.Fields.Add(poolField);

                                        // Initialize pool
                                        var cctor = GetOrCreateClassConstructor(closureType);
                                        var ilProcessor2 = cctor.Body.GetILProcessor();
                                        var retInstruction = cctor.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Ret);
                                        ilProcessor2.InsertBefore(retInstruction, ilProcessor2.Create(OpCodes.Ldnull));
                                        ilProcessor2.InsertBefore(retInstruction, ilProcessor2.Create(OpCodes.Ldftn, createPoolItemMethod));
                                        ilProcessor2.InsertBefore(retInstruction, ilProcessor2.Create(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(mscorlibAssembly.MainModule.GetType("System.Func`1").Methods.FirstOrDefault(x => x.Name == ".ctor")).MakeGeneric(closureInstanceType)));
                                        ilProcessor2.InsertBefore(retInstruction, ilProcessor2.Create(OpCodes.Newobj, context.Assembly.MainModule.ImportReference(poolType.Methods.FirstOrDefault(x => x.Name == ".ctor")).MakeGeneric(closureInstanceType)));
                                        ilProcessor2.InsertBefore(retInstruction, ilProcessor2.Create(OpCodes.Stfld, poolField));

                                        //// Retrieve closure from pool, instead of allocating
                                        //var acquireClosure = ilProcessor.Create(OpCodes.Callvirt, context.Assembly.MainModule.ImportReference(poolType.Methods.FirstOrDefault(x => x.Name == "Acquire")).MakeGeneric(closureType));
                                        //ilProcessor.Replace(closureAllocation, acquireClosure);
                                        //ilProcessor.InsertBefore(acquireClosure, ilProcessor.Create(OpCodes.Ldfld, poolField));

                                        //// Get delegate from closure, instead of allocating
                                        //ilProcessor.Remove(functionPointerInstruction);
                                        //ilProcessor.Replace(delegateAllocation, ilProcessor.Create(OpCodes.Ldfld, delegateField));
                                    }
                                }
                                
                                changed = true;
                            }
                        }
                    }
                }
            }

            return changed;
        }

        private MethodDefinition GetOrCreateClassConstructor(TypeDefinition type)
        {
            var cctor = type.Methods.FirstOrDefault(x => x.Name == ".cctor");
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor", MethodAttributes.Private
                    | MethodAttributes.HideBySig
                    | MethodAttributes.Static
                    | MethodAttributes.Assembly
                    | MethodAttributes.SpecialName
                    | MethodAttributes.RTSpecialName, type.Module.TypeSystem.Void);
                type.Methods.Add(cctor);
            }

            var retInstruction = cctor.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Ret);
            if (retInstruction == null)
            {
                var ilProcessor = cctor.Body.GetILProcessor();
                ilProcessor.Emit(OpCodes.Ret);
            }

            return cctor;
        }

        private TypeReference ChangeGenericArguments(AssemblyProcessorContext context, TypeReference type, TypeReference relativeType)
        {
            var genericInstance = type as GenericInstanceType;
            if (genericInstance == null)
                return type;

            var genericArguments = new List<TypeReference>();
            foreach (var genericArgument in genericInstance.GenericArguments)
            {
                if (genericArgument.IsGenericParameter)
                {
                    var genericParameter = GetGenericParameterForArgument(relativeType, genericArgument);
                    if (genericParameter != null)
                    {
                        genericArguments.Add(genericParameter);
                    }
                }
                else
                {
                    var newGenericArgument = ChangeGenericArguments(context, genericArgument, relativeType);
                    genericArguments.Add(newGenericArgument);
                }
            }

            if (genericArguments.Count != genericInstance.GenericArguments.Count)
            {
                throw new InvalidOperationException("Could not resolve generic arguments");
            }

            return context.Assembly.MainModule.ImportReference(genericInstance.Resolve()).MakeGenericInstanceType(genericArguments.ToArray());
        }

        private GenericParameter GetGenericParameterForArgument(TypeReference type, TypeReference genericArgument)
        {
            var relativeGenericInstance = type as GenericInstanceType;
            if (relativeGenericInstance == null)
                return null;

            for (int index = 0; index < relativeGenericInstance.GenericArguments.Count; index++)
            {
                var relativeGenericArgument = relativeGenericInstance.GenericArguments[index];
                if (relativeGenericArgument == genericArgument)
                {
                    var genericParameter = relativeGenericInstance.Resolve().GenericParameters[index];
                    return genericParameter;
                }
                else
                {
                    var childParameter = GetGenericParameterForArgument(relativeGenericArgument, genericArgument);
                    if (childParameter != null)
                        return childParameter;
                }
            }

            return null;
        }
    }
}