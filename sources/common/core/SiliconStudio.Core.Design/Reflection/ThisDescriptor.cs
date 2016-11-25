// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A <see cref="IMemberDescriptor"/> for the 'this' keyword.
    /// </summary>
    public class ThisDescriptor : MemberDescriptorBase
    {
        public static readonly ThisDescriptor Default = new ThisDescriptor("this");

        public ThisDescriptor(string name)
            : base(name)
        {
        }

        public override Type Type => typeof(object);

        public override object Get(object thisObject)
        {
            return thisObject;
        }

        public override void Set(object thisObject, object value)
        {
            throw new NotSupportedException();
        }

        public override bool IsPublic => false;

        public override bool HasSet => false;

        public override IEnumerable<T> GetCustomAttributes<T>(bool inherit)
        {
            return Enumerable.Empty<T>();
        }
    }
}
