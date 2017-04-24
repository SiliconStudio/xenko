// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Reflection
{
    public static class ModuleRuntimeHelpers
    {
        public static void RunModuleConstructor([NotNull] Module module)
        {
#if SILICONSTUDIO_PLATFORM_UWP || SILICONSTUDIO_RUNTIME_CORECLR
            // Initialize first type
            // TODO: Find a type without actual .cctor if possible, to avoid side effects
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(module.Assembly.DefinedTypes.First().AsType().TypeHandle);
#else
            System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(module.ModuleHandle);
#endif
        }
    }
}
