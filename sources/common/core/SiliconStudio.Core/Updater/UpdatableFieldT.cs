using System;

namespace SiliconStudio.Core.Updater
{
    public class UpdatableField<T> : UpdatableField
    {
        public static Func<UpdatableMember> StaticCreateMemberElement;

        public override UpdatableMember CreateMemberElement()
        {
            return StaticCreateMemberElement();
        }

        public UpdatableField(int offset)
        {
            Offset = offset;
            Size = Interop.SizeOf<T>();
        }

        public override Type MemberType
        {
            get { return typeof(T); }
        }

        public override void SetStruct(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            // Target
            ldarg obj

            // Load source (unboxed pointer)
            ldarg data
            unbox !T

            // *obj = *source
            cpobj !T
#endif
            throw new NotImplementedException();
        }
    }
}