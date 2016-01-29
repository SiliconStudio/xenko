// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SiliconStudio.AssemblyProcessor;

namespace SiliconStudio.AssemblyProcessor
{
    internal class ModuleInitializerProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            var assembly = context.Assembly;
            var moduleInitializers = new List<MethodReference>();

            // Generate a module initializer for all types, including nested types
            foreach (var type in assembly.EnumerateTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method.CustomAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.ModuleInitializerAttribute"))
                    {
                        moduleInitializers.Add(method);
                    }
                }
            }

            if (moduleInitializers.Count == 0)
                return false;

            // Get or create module static constructor
            Instruction returnInstruction;
            var staticConstructor = OpenModuleConstructor(assembly, out returnInstruction);

            var il = staticConstructor.Body.GetILProcessor();
            
            var newReturnInstruction = Instruction.Create(returnInstruction.OpCode);
            newReturnInstruction.Operand = returnInstruction.Operand;

            returnInstruction.OpCode = OpCodes.Nop;
            returnInstruction.Operand = null;

            staticConstructor.Body.SimplifyMacros();
            foreach (var moduleInitializer in moduleInitializers)
            {
                il.Append(Instruction.Create(OpCodes.Call, moduleInitializer));
            }
            il.Append(newReturnInstruction);
            staticConstructor.Body.OptimizeMacros();

            return true;
        }

        public static MethodDefinition OpenModuleConstructor(AssemblyDefinition assembly, out Instruction returnInstruction)
        {
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
            returnInstruction = staticConstructor.Body.Instructions.Last();

            return staticConstructor;
        }
    }
}