using System;
using System.Reflection;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Provide a custom implementation to access a member by the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableCustomAccessor : UpdatablePropertyBase
    {
        public abstract object GetObject(IntPtr obj);
        public abstract void SetObject(IntPtr obj, object data);

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
                return UpdateOperationType.ConditionalSetObjectCustom;
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
                return UpdateOperationType.EnterObjectCustom;
            }
        }
    }
}