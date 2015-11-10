using System;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Shared class between <see cref="UpdatableProperty"/> and <see cref="UpdatableCustomAccessor"/>.
    /// </summary>
    public abstract class UpdatablePropertyBase : UpdatableMember
    {
        public abstract void GetBlittable(IntPtr obj, IntPtr data);
        public abstract void SetBlittable(IntPtr obj, IntPtr data);
        public abstract void SetStruct(IntPtr obj, object data);
        public abstract IntPtr GetStructAndUnbox(IntPtr obj, object data);

        internal abstract UpdateOperationType GetSetOperationType();
        internal abstract UpdateOperationType GetEnterOperationType();
    }
}