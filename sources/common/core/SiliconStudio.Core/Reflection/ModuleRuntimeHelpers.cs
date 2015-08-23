// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    public static class ModuleRuntimeHelpers
    {
        public static void RunModuleConstructor(Module module)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME || SILICONSTUDIO_RUNTIME_CORECLR
            // Initialize first type
            // TODO: Find a type without actual .cctor if possible, to avoid side effects
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(module.Assembly.DefinedTypes.First().AsType().TypeHandle);
#else
            System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(module.ModuleHandle);
#endif
        }
    }
}