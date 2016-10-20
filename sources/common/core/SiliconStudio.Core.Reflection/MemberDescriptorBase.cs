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
        private MemberInfo memberInfo;
        private StringComparer defaultNameComparer;

        protected MemberDescriptorBase(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Name = name;
            OriginalName = name;
        }

        protected MemberDescriptorBase(MemberInfo memberInfo, StringComparer defaultNameComparer)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));

            MemberInfo = memberInfo;
            Name = MemberInfo.Name;
            DeclaringType = memberInfo.DeclaringType;
            DefaultNameComparer = defaultNameComparer;
        }

        // TODO: turn the public setters internal or protected

        public string Name { get; set; }
        public string OriginalName { get; }
        public StringComparer DefaultNameComparer { get; }
        public abstract Type Type { get; }
        public int? Order { get; set; }

        /// <summary>
        /// Gets the type of the declaring this member.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public Type DeclaringType { get; }

        public ITypeDescriptor TypeDescriptor { get; set; }

        public DataMemberMode Mode { get; set; }
        public abstract object Get(object thisObject);
        public abstract void Set(object thisObject, object value);
        /// <summary>
        /// Gets whether this member has a public getter.
        /// </summary>
        public abstract bool IsPublic { get; }
        public abstract bool HasSet { get; }
        public abstract IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;

        /// <summary>
        /// Gets the member information.
        /// </summary>
        /// <value>The member information.</value>
        public MemberInfo MemberInfo { get; }

        public Func<object, bool> ShouldSerialize { get; set; }

        public List<string> AlternativeNames { get; set; }

        public object Tag { get; set; }
    }
}
