using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Entities
{
    // TODO: Move it to Assets? Or Core/Core.Design?
    public class ConvertedDescriptor : MemberDescriptorBase
    {
        private Type type;
        private bool hasSet;
        private object value;

        public override Type Type
        {
            get { return value.GetType(); }
        }

        public ConvertedDescriptor(ITypeDescriptorFactory factory, string name, object value) : base(factory, name)
        {
            this.value = value;
            this.TypeDescriptor = Factory.Find(value.GetType());
        }

        public override object Get(object thisObject)
        {
            return value;
        }

        public override void Set(object thisObject, object value)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> GetCustomAttributes<T>(bool inherit)
        {
            return Enumerable.Empty<T>();
        }

        public override bool HasSet
        {
            get { return false; }
        }
    }
}