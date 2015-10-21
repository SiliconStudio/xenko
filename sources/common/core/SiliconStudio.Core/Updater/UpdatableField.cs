// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Updater
{
    public abstract class UpdatableField : UpdatableMember
    {
        public int Offset;
        public int Size;

        public abstract void SetStruct(IntPtr obj, object data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetObject(IntPtr obj)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldind.ref
            ret
#endif
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetObject(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg data
            stind.ref
            ret
#endif
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetBlittable(IntPtr obj, IntPtr data)
        {
            Interop.memcpy((void*)obj, (void*)data, Size);
        }

        public UpdateOperationType GetSetOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                if (BlittableHelper.IsBlittable(MemberType))
                {
                    if (Size == 4)
                        return UpdateOperationType.ConditionalSetBlittableField4;
                    if (Size == 8)
                        return UpdateOperationType.ConditionalSetBlittableField8;
                    if (Size == 12)
                        return UpdateOperationType.ConditionalSetBlittableField12;
                    if (Size == 16)
                        return UpdateOperationType.ConditionalSetBlittableField16;

                    return UpdateOperationType.ConditionalSetBlittableField;
                }

                return UpdateOperationType.ConditionalSetStructField;
            }
            else
            {
                return UpdateOperationType.ConditionalSetObjectField;
            }
        }
    }
}