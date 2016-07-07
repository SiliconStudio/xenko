// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace SiliconStudio.AssemblyProcessor
{
    internal class ParameterKeyProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            var assembly = context.Assembly;
            var fields = new List<FieldDefinition>();

            var mscorlibAssembly = CecilExtensions.FindCorlibAssembly(assembly);
            if (mscorlibAssembly == null)
                throw new InvalidOperationException("Missing mscorlib.dll from assembly");

            MethodDefinition parameterKeysMergeMethod = null;
            TypeDefinition assemblyEffectKeysAttributeType = null;
            var getTypeFromHandleMethod = new Lazy<MethodReference>(() =>
            {
                // Find Type.GetTypeFromHandle
                var typeType = mscorlibAssembly.MainModule.GetTypeResolved(typeof(Type).FullName);
                return assembly.MainModule.ImportReference(typeType.Methods.First(x => x.Name == "GetTypeFromHandle"));
            });

            var effectKeysToMerge = new List<Tuple<TypeDefinition, FieldReference>>();

            foreach (var type in assembly.MainModule.GetTypes())
            {
                fields.Clear();

                foreach (var field in type.Fields.Where(x => x.IsStatic))
                {
                    var fieldBaseType = field.FieldType;
                    while (fieldBaseType != null)
                    {
                        if (fieldBaseType.FullName == "SiliconStudio.Xenko.Rendering.ParameterKey")
                            break;

                        var resolvedFieldBaseType = fieldBaseType.Resolve();
                        if (resolvedFieldBaseType == null)
                        {
                            fieldBaseType = null;
                            break;
                        }

                        fieldBaseType = resolvedFieldBaseType.BaseType;
                    }

                    if (fieldBaseType == null)
                        continue;

                    fields.Add(field);
                }

                if (fields.Count == 0)
                    continue;

                // ParameterKey present means we should have a static cctor.
                var cctor = type.GetStaticConstructor();
                if (cctor == null)
                    continue;

                // Load necessary SiliconStudio.Xenko methods/attributes
                if (parameterKeysMergeMethod == null)
                {
                    AssemblyDefinition xenkoEngineAssembly;
                    try
                    {
                        xenkoEngineAssembly = assembly.Name.Name == "SiliconStudio.Xenko"
                            ? assembly
                            : context.AssemblyResolver.Resolve("SiliconStudio.Xenko");
                    }
                    catch (Exception)
                    {
                        context.Log.WriteLine("Error, cannot find [SiliconStudio.Xenko] assembly for processing ParameterKeyProcessor");
                        // We can't generate an exception, so we are just returning. It means that SiliconStudio.Xenko has not been generated so far.
                        return true;
                    }

                    var parameterKeysType = xenkoEngineAssembly.MainModule.GetTypes().First(x => x.Name == "ParameterKeys");
                    parameterKeysMergeMethod = parameterKeysType.Methods.First(x => x.Name == "Merge");
                    assemblyEffectKeysAttributeType = xenkoEngineAssembly.MainModule.GetTypes().First(x => x.Name == "AssemblyEffectKeysAttribute");
                }

                var cctorInstructions = cctor.Body.Instructions;

                // Every field which has a stsfld instruction will be processed
                for (int i = 0; i < cctorInstructions.Count; ++i)
                {
                    var fieldInstruction = cctorInstructions[i];

                    if (fieldInstruction.OpCode == OpCodes.Stsfld
                        && fields.Contains(fieldInstruction.Operand))
                    {
                        var activeField = (FieldReference)fieldInstruction.Operand;
                        effectKeysToMerge.Add(Tuple.Create(type, activeField));
                    }
                }
            }

            if (effectKeysToMerge.Count > 0)
            {
                // Add [AssemblyEffectKeysAttribute] to the assembly
                assembly.CustomAttributes.Add(new CustomAttribute(assembly.MainModule.ImportReference(assemblyEffectKeysAttributeType.GetConstructors().First(x => !x.HasParameters))));

                // Get or create module static constructor
                var voidType = assembly.MainModule.TypeSystem.Void;
                var moduleClass = assembly.MainModule.Types.First(t => t.Name == "<Module>");
                var staticConstructor = moduleClass.GetStaticConstructor();
                if (staticConstructor == null)
                {
                    staticConstructor = new MethodDefinition(".cctor",
                                                             MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                             voidType);
                    staticConstructor.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));

                    moduleClass.Methods.Add(staticConstructor);
                }

                var il = staticConstructor.Body.GetILProcessor();

                var returnInstruction = staticConstructor.Body.Instructions.Last();
                var newReturnInstruction = Instruction.Create(returnInstruction.OpCode);
                newReturnInstruction.Operand = returnInstruction.Operand;

                returnInstruction.OpCode = OpCodes.Nop;
                returnInstruction.Operand = null;

                // TODO: Move that to a sub function?
                // Call PropertyKey.Merge on every keys
                staticConstructor.Body.SimplifyMacros();
                foreach (var effectKeysStaticConstructor in effectKeysToMerge)
                {
                    var type = effectKeysStaticConstructor.Item1;
                    var activeField = effectKeysStaticConstructor.Item2;

                    var keyClassName = type.Name;
                    if (keyClassName.EndsWith("Keys"))
                        keyClassName = keyClassName.Substring(0, keyClassName.Length - 4);

                    keyClassName += '.';

                    il.Append(Instruction.Create(OpCodes.Ldsfld, activeField));
                    il.Append(Instruction.Create(OpCodes.Ldtoken, type));
                    il.Append(Instruction.Create(OpCodes.Call, getTypeFromHandleMethod.Value));
                    il.Append(Instruction.Create(OpCodes.Ldstr, keyClassName + activeField.Name));
                    il.Append(Instruction.Create(OpCodes.Call, assembly.MainModule.ImportReference(parameterKeysMergeMethod)));
                    il.Append(Instruction.Create(OpCodes.Castclass, activeField.FieldType));
                    il.Append(Instruction.Create(OpCodes.Stsfld, activeField));
                }

                il.Append(newReturnInstruction);
                staticConstructor.Body.OptimizeMacros();
            }

            return true;
        }
    }
}