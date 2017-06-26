// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using Mono.Cecil.Rocks;

namespace SiliconStudio.AssemblyProcessor
{
    internal class InitLocalsProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            bool changed = false;
            foreach (var type in context.Assembly.MainModule.GetAllTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method.CustomAttributes.Any(x => x.AttributeType.FullName == "SiliconStudio.Core.IL.RemoveInitLocalsAttribute"))
                    {
                        if (method.Body == null)
                        {
                            throw new InvalidOperationException($"Trying to remove initlocals from method {method.FullName} without body.");
                        }

                        method.Body.InitLocals = false;
                        changed = true;
                    }
                }
            }

            return changed;
        }
    }
}
