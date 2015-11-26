namespace SiliconStudio.Xenko.NativeBridge
{
    public static class NativeBridge
    {
#if SILICONSTUDIO_PLATFORM_IOS
        public const string Library = "__Internal";
#else
        public const string Library = "SiliconStudio.Xenko.Native.dll";
#endif
    }
}