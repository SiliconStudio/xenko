// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Base class for <see cref="IMemberDescriptor"/> for a <see cref="MemberInfo"/>
    /// </summary>
    public abstract class MemberDescriptorBase : IMemberDescriptor
    {
        protected MemberDescriptorBase(ITypeDescriptorFactory factory, string name)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (name == null) throw new ArgumentNullException("name");

            Factory = factory;
            Name = name;
        }

        protected MemberDescriptorBase(ITypeDescriptorFactory factory, MemberInfo memberInfo)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");

            Factory = factory;
            MemberInfo = memberInfo;
            Name = MemberInfo.Name;
            DeclaringType = memberInfo.DeclaringType;
        }

        public string Name { get; internal set; }
        public abstract Type Type { get; }
        public int? Order { get; internal set; }

        /// <summary>
        /// Gets the type of the declaring this member.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public Type DeclaringType { get; private set; }

        public ITypeDescriptor TypeDescriptor { get; protected set; }

        public ITypeDescriptorFactory Factory { get; private set; }
        public DataMemberMode Mode { get; internal set; }
        public abstract object Get(object thisObject);
        public abstract void Set(object thisObject, object value);
        public abstract bool HasSet { get; }
        public abstract IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;
        public DataStyle Style { get; internal set; }

        /// <summary>
        /// Gets the member information.
        /// </summary>
        /// <value>The member information.</value>
        public MemberInfo MemberInfo { get; private set; }
    }
}