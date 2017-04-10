using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Audio
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Make sure that this assembly is registered
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
        }
    }
}