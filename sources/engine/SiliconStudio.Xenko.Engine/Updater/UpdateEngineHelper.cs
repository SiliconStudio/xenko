using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Various helper functions for the <see cref="UpdateEngine"/>.
    /// </summary>
    internal static unsafe class UpdateEngineHelper
    {
        public static int ArrayFirstElementOffset = ComputeArrayFirstElementOffset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr ObjectToPtr(object obj)
        {
#if IL
            ldarg obj
            conv.i
            ret
#endif
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PtrToObject<T>(IntPtr obj) where T : class
        {
            object convObj; // TEMP XAMARIN AOT FIX -- DOES NOT WORK FOR VALUE TYPE PROPERTIES
#if IL
            ldarg obj
            stloc convObj // TEMP XAMARIN AOT FIX -- DOES NOT WORK FOR VALUE TYPE PROPERTIES
            ldloc convObj // TEMP XAMARIN AOT FIX -- DOES NOT WORK FOR VALUE TYPE PROPERTIES
            ret
#endif
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Unbox<T>(object obj)
        {
#if IL
            ldarg obj
            unbox !!T
            ret
#endif
            throw new NotImplementedException();
        }

        static int ComputeArrayFirstElementOffset()
        {
            var testArray = new int[1];
            int result = 0;
            fixed (int* testArrayStart = testArray)
            {
                var testArrayObjectStart = ObjectToPtr(testArray);
                return (int)((byte*)testArrayStart - (byte*)testArrayObjectStart);
            }
        }
    }
}