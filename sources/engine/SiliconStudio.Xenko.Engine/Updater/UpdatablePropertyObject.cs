using System;

namespace SiliconStudio.Xenko.Updater
{
    public class UpdatablePropertyObject<T> : UpdatableProperty
    {
        public UpdatablePropertyObject(IntPtr getter, IntPtr setter) : base(getter, setter)
        {
        }

        public override Type MemberType
        {
            get { return typeof(T); }
        }

        public override void GetBlittable(IntPtr obj, IntPtr data)
        {
            throw new NotImplementedException();
        }

        public override void SetBlittable(IntPtr obj, IntPtr data)
        {
            throw new NotImplementedException();
        }

        public override void SetStruct(IntPtr obj, object data)
        {
            throw new NotImplementedException();
        }

        public override IntPtr GetStructAndUnbox(IntPtr obj, object data)
        {
            throw new NotImplementedException();
        }
    }
}