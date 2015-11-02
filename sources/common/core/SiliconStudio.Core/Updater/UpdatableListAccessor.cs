using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Updater
{
    abstract class UpdatableListAccessor : UpdatableCustomAccessor
    {
        public readonly int Index;

        protected UpdatableListAccessor(int index)
        {
            Index = index;
        }
    }

    class UpdatableListAccessor<T> : UpdatableListAccessor
    {
        public static Func<UpdatableMember> StaticCreateMemberElement;

        public UpdatableListAccessor(int index) : base(index)
        {
        }

        public override UpdatableMember CreateMemberElement()
        {
            return StaticCreateMemberElement();
        }

        public override Type MemberType
        {
            get { return typeof(T); }
        }

        public override IntPtr GetStructAndUnbox(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg data
            unbox !T
            dup
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            callvirt instance !T class [mscorlib]System.Collections.Generic.IList`1<!T>::get_Item(int32)
            stobj !T
            ret
#endif
            throw new NotImplementedException();
        }

        public override void GetBlittable(IntPtr obj, IntPtr data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg data
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            callvirt instance !T class [mscorlib]System.Collections.Generic.IList`1<!T>::get_Item(int32)
            stobj !T
            ret
#endif
            throw new NotImplementedException();
        }

        public override void SetStruct(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            ldarg data
            unbox.any !T
            callvirt instance void class [mscorlib]System.Collections.Generic.IList`1<!T>::set_Item(int32, !0)
            ret
#endif
            throw new NotImplementedException();
        }

        public override void SetBlittable(IntPtr obj, IntPtr data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            ldarg data
            ldobj !T
            callvirt instance void class [mscorlib]System.Collections.Generic.IList`1<!T>::set_Item(int32, !0)
            ret
#endif
            throw new NotImplementedException();
        }

        public override object GetObject(IntPtr obj)
        {
#if IL
            // Use method to set testI
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            callvirt instance !T class [mscorlib]System.Collections.Generic.IList`1<!T>::get_Item(int32)
            ret
#endif
            throw new NotImplementedException();
        }

        public override void SetObject(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg.0
            ldfld int32 SiliconStudio.Core.Updater.UpdatableListAccessor::Index
            ldarg data
            callvirt instance void class [mscorlib]System.Collections.Generic.IList`1<!T>::set_Item(int32, !0)
            ret
#endif
            throw new NotImplementedException();
        }
    }
}