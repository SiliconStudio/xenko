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

        public UpdateOperationType GetOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                if (BlittableHelper.IsBlittable(MemberType))
                    return UpdateOperationType.ConditionalSetBlittableField;

                return UpdateOperationType.ConditionalSetStructField;
            }
            else
            {
                return UpdateOperationType.ConditionalSetObjectField;
            }
        }
    }
}