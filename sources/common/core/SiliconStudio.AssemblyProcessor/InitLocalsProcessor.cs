// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

namespace SiliconStudio.AssemblyProcessor
{
    internal class InitLocalsProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            bool changed = false;
            foreach (var type in context.Assembly.EnumerateTypes())
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