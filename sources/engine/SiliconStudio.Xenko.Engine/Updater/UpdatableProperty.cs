using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines how to set and get values from a property for the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableProperty : UpdatablePropertyBase
    {
        public IntPtr Getter;
        public IntPtr Setter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetObject(IntPtr obj)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg.0
            ldfld native int class SiliconStudio.Xenko.Updater.UpdatableProperty::Getter
            calli instance object()
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
            ldarg.0
            ldfld native int class SiliconStudio.Xenko.Updater.UpdatableProperty::Setter
            calli instance void(object)
            ret
#endif
            throw new NotImplementedException();
        }

        internal override UpdateOperationType GetSetOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                if (BlittableHelper.IsBlittable(MemberType))
                    return UpdateOperationType.ConditionalSetBlittablePropertyBase;

                return UpdateOperationType.ConditionalSetStructPropertyBase;
            }
            else
            {
                return UpdateOperationType.ConditionalSetObjectProperty;
            }
        }

        internal override UpdateOperationType GetEnterOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                return UpdateOperationType.EnterStructPropertyBase;
            }
            else
            {
                return UpdateOperationType.EnterObjectProperty;
            }
        }
    }
}