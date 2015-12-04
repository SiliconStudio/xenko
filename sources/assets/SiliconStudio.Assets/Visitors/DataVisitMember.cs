// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// A diff element for a member (field or property) of a class.
    /// </summary>
    public sealed class DataVisitMember : DataVisitNode
    {
        private readonly IMemberDescriptor memberDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitMember" /> class.
        /// </summary>
        /// <param name="memberDescriptor">The member descriptor.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// instanceDescriptor
        /// or
        /// memberDescriptor
        /// </exception>
        public DataVisitMember(object value, IMemberDescriptor memberDescriptor)
            : base(value, memberDescriptor.TypeDescriptor)
        {
            if (memberDescriptor == null) throw new ArgumentNullException("memberDescriptor");
            this.memberDescriptor = memberDescriptor;
        }

        /// <summary>
        /// Gets the member descriptor.
        /// </summary>
        /// <value>The member descriptor.</value>
        public IMemberDescriptor MemberDescriptor
        {
            get
            {
                return memberDescriptor;
            }
        }

        public override void SetValue(object newValue)
        {
            MemberDescriptor.Set(Parent.Instance, newValue);
            Instance = newValue;

            if (Parent.InstanceType.IsStruct() && Parent is DataVisitMember)
            {
                ((DataVisitMember)Parent).SetValue(Parent.Instance);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", MemberDescriptor.Name, Instance ?? "null");
        }

        public override void RemoveValue()
        {
            SetValue(null);
        }

        public override DataVisitNode CreateWithEmptyInstance()
        {
            return new DataVisitMember(null, MemberDescriptor);
        }
    }
}