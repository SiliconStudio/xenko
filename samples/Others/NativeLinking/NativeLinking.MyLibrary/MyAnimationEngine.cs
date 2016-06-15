using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace NativeLinking.LibraryWrapper
{
    public class MyAnimationEngine
    {

        private const string LibraryName =
#if SILICONSTUDIO_PLATFORM_IOS
            "__Internal";
#else
            "NativeLibrary";
#endif

        static MyAnimationEngine()
        {
            //This step is necessary under Windows Desktop platform to figure the arch
            SiliconStudio.Core.NativeLibrary.PreloadLibrary(LibraryName + ".dll");
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetCurrentPositionNative(float timeInSeconds, out Vector3 position);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetCurrentRotationNative(float timeInSeconds, out Vector3 rotation);

        public Vector3 GetCurrentPosition(float timeInSeconds)
        {
            Vector3 value;
            GetCurrentPositionNative(timeInSeconds, out value);

            return value;
        }

        public Vector3 GetCurrentRotation(float timeInSeconds)
        {
            Vector3 value;
            GetCurrentRotationNative(timeInSeconds, out value);

            return value;
        }
    }
}
