using System;
using System.Reflection;

namespace SiliconStudio.Core.Updater
{
    /// <summary>
    /// Provide a custom implementation to access a member by the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableCustomAccessor : UpdatablePropertyBase
    {
        public abstract object GetObject(IntPtr obj);
        public abstract void SetObject(IntPtr obj, object data);

        internal override UpdateOperationType GetOperationType()
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
    }
}