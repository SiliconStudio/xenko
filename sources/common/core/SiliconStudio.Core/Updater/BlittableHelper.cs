using System;
using System.Reflection;
using System.Runtime.InteropServices;

// TODO: We should switch to something determined at compile time with assembly processor?
namespace SiliconStudio.Core.Updater
{
    public static class BlittableHelper
    {
        public static bool IsBlittable(Type type)
        {
            try
            {
                // Class test
                if (!type.GetTypeInfo().IsValueType)
                    return false;

                // Non-blittable types cannot allocate pinned handle
                GCHandle.Alloc(Activator.CreateInstance(type), GCHandleType.Pinned).Free();
                return true;
            }
            catch { }

            return false;
        }
    }
}