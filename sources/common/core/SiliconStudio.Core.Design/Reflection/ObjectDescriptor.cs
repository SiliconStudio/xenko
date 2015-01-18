// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
// -----------------------------------------------------------------------------------
// The following code is a partial port of YamlSerializer
// https://yamlserializer.codeplex.com
// -----------------------------------------------------------------------------------
// Copyright (c) 2009 Osamu TAKEUCHI <osamu@big.jp>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Default implementation of a <see cref="ITypeDescriptor"/>.
    /// </summary>
    public class ObjectDescriptor : ITypeDescriptor
    {
        private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();
        protected static readonly string SystemCollectionsNamespace = typeof(int).Namespace;

        private static readonly object[] EmptyObjectArray = new object[0];
        private readonly ITypeDescriptorFactory factory;
        private readonly Type type;
        private IMemberDescriptor[] members;
        private Dictionary<string, IMemberDescriptor> mapMembers;
        private DataStyle style;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="System.InvalidOperationException">Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}].ToFormat(type.FullName, member, existingMember)</exception>
        public ObjectDescriptor(ITypeDescriptorFactory factory, Type type)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (type == null) throw new ArgumentNullException("type");

            this.factory = factory;
            Category = DescriptorCategory.Object;
            this.AttributeRegistry = factory.AttributeRegistry;
            this.type = type;
            var styleAttribute = AttributeRegistry.GetAttribute<DataStyleAttribute>(type);
            this.style = styleAttribute != null ? styleAttribute.Style : DataStyle.Any;
            this.IsCompilerGenerated = AttributeRegistry.GetAttribute<CompilerGeneratedAttribute>(type) != null;
        }

        public virtual void Initialize()
        {
            if (members != null)
                return;

            var memberList = PrepareMembers();

            // Sort members by name
            // This is to make sure that properties/fields for an object 
            // are always displayed in the same order
            memberList.Sort(SortMembers);

            // Free the member list
            this.members = memberList.ToArray();

            // If no members found, we don't need to build a dictionary map
            if (members.Length <= 0) return;

            mapMembers = new Dictionary<string, IMemberDescriptor>(members.Length);

            foreach (var member in members)
            {
                IMemberDescriptor existingMember;
                if (mapMembers.TryGetValue(member.Name, out existingMember))
                {
                    throw new InvalidOperationException("Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}]".ToFormat(type.FullName, member, existingMember));
                }

                mapMembers.Add(member.Name, member);
            }
        }

        private int SortMembers(IMemberDescriptor left, IMemberDescriptor right)
        {
            // If order is defined, first order by order
            if (left.Order.HasValue || right.Order.HasValue)
            {
                var leftOrder = left.Order.HasValue ? left.Order.Value : int.MaxValue;
                var rightOrder = right.Order.HasValue ? right.Order.Value : int.MaxValue;
                return leftOrder.CompareTo(rightOrder);
            }

            // else order by name
            return string.CompareOrdinal(left.Name, right.Name);
        }

        protected IAttributeRegistry AttributeRegistry { get; private set; }

        public ITypeDescriptorFactory Factory
        {
            get
            {
                return factory;
            }
        }

        public Type Type
        {
            get
            {
                return type;
            }
        }

        public IEnumerable<IMemberDescriptor> Members
        {
            get
            {
                return members;
            }
        }

        public int Count
        {
            get
            {
                return members.Length;
            }
        }

        public bool HasMembers
        {
            get
            {
                return members.Length > 0;
            }
        }

        public DescriptorCategory Category
        {
            get;
            protected set;
        }

        public DataStyle Style
        {
            get
            {
                return style;
            }
        }

        public IMemberDescriptor this[string name]
        {
            get
            {
                IMemberDescriptor member = null;
                if (mapMembers != null)
                {
                    mapMembers.TryGetValue(name, out member);
                }
                return member;
            }
        }

        public bool IsCompilerGenerated { get; private set; }

        public bool Contains(string memberName)
        {
            return mapMembers != null && mapMembers.ContainsKey(memberName);
        }

        protected virtual List<IMemberDescriptor> PrepareMembers()
        {
            if (type == typeof(Type))
            {
                return EmptyMembers;
            }

            // Add all public properties with a readable get method
            var memberList = (from propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                              where
                                  propertyInfo.CanRead && propertyInfo.GetGetMethod(false) != null &&
                                  propertyInfo.GetIndexParameters().Length == 0 &&
                                  IsMemberToVisit(propertyInfo)
                              select new PropertyDescriptor(Factory, propertyInfo)
                              into member
                              where PrepareMember(member)
                              select member).Cast<IMemberDescriptor>().ToList();

            // Add all public fields
            memberList.AddRange((from fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                 where fieldInfo.IsPublic &&
                                  IsMemberToVisit(fieldInfo)
                                 select new FieldDescriptor(Factory, fieldInfo)
                                 into member
                                 where PrepareMember(member)
                                 select member));

            return memberList;
        }

        protected bool IsMemberToVisit(MemberInfo memberInfo)
        {
            // Remove all SyncRoot from members
            if (memberInfo is PropertyInfo && memberInfo.Name == "SyncRoot" && memberInfo.DeclaringType != null && (memberInfo.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace))
            {
                return false;
            }

            Type memberType = null;
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                memberType = fieldInfo.FieldType;
            }
            else
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    memberType = propertyInfo.PropertyType;
                }
            }

            if (memberType  != null)
            {
                if (typeof(Delegate).IsAssignableFrom(memberType))
                {
                    return false;
                }
            }


            // Member is not displayed if there is a YamlIgnore attribute on it
            if (AttributeRegistry.GetAttribute<DataMemberIgnoreAttribute>(memberInfo) != null)
                return false;

            return true;
        }

        protected virtual bool PrepareMember(MemberDescriptorBase member)
        {
            var memberType = member.Type;

            // If the member has a set, this is a conventional assign method
            if (member.HasSet)
            {
                member.Mode = DataMemberMode.Assign;
            }
            else
            {
                // Else we cannot only assign its content if it is a class
                member.Mode = (memberType != typeof(string) && memberType.IsClass) || memberType.IsInterface || type.IsAnonymous() ? DataMemberMode.Content : DataMemberMode.Never;
            }

            // Gets the style
            var styleAttribute = AttributeRegistry.GetAttribute<DataStyleAttribute>(member.MemberInfo);
            member.Style = styleAttribute != null ? styleAttribute.Style : DataStyle.Any;

            // Handle member attribute
            var memberAttribute = AttributeRegistry.GetAttribute<DataMemberAttribute>(member.MemberInfo);
            if (memberAttribute != null)
            {
                if (!member.HasSet)
                {
                    if (memberAttribute.Mode == DataMemberMode.Assign ||
                        (memberType.IsValueType && member.Mode == DataMemberMode.Content))
                        throw new ArgumentException("{0} {1} is not writeable by {2}.".ToFormat(memberType.FullName, member.Name, memberAttribute.Mode.ToString()));
                }

                if (memberAttribute.Mode != DataMemberMode.Default)
                {
                    member.Mode = memberAttribute.Mode;
                }
                member.Order = memberAttribute.Order;
            }

            if (member.Mode == DataMemberMode.Binary)
            {
                if (!memberType.IsArray)
                    throw new InvalidOperationException("{0} {1} of {2} is not an array. Can not be serialized as binary."
                                                            .ToFormat(memberType.FullName, member.Name, type.FullName));
                if (!memberType.GetElementType().IsPureValueType())
                    throw new InvalidOperationException("{0} is not a pure ValueType. {1} {2} of {3} can not serialize as binary.".ToFormat(memberType.GetElementType(), memberType.FullName, member.Name, type.FullName));
            }

            // If this member cannot be serialized, remove it from the list
            if (member.Mode == DataMemberMode.Never)
            {
                return false;
            }

            if (memberAttribute != null && !string.IsNullOrEmpty(memberAttribute.Name))
            {
                member.Name = memberAttribute.Name;
            }

            return true;
        }
    }
}