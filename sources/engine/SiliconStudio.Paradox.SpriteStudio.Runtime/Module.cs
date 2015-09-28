using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Engine
{
    public static class Module
    {
        [ModuleInitializer]
        public static void InitializeModule()
        {
            //RegisterPlugin(typeof(SpriteStudioPlugin));
            AssemblyRegistry.Register(typeof(Module).GetTypeInfo().Assembly, AssemblyCommonCategories.Assets);
        }
    }
}