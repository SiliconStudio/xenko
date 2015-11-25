using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    public static class Module
    {
        [ModuleInitializer]
        public static void InitializeModule()
        {
            NativeLibrary.PreloadLibrary("SiliconStudio.Xenko.Native.dll");
        }
    }
}