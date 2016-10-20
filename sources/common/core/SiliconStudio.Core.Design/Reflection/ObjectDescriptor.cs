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
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Default implementation of a <see cref="ITypeDescriptor"/>.
    /// </summary>
    public class ObjectDescriptor : ObjectDescriptorBase
    {
        private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();

        private readonly ITypeDescriptorFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="System.InvalidOperationException">Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}].ToFormat(type.FullName, member, existingMember)</exception>
        public ObjectDescriptor(ITypeDescriptorFactory factory, Type type)
            : base(factory.AttributeRegistry, type)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (type == null) throw new ArgumentNullException(nameof(type));
            this.factory = factory;
        }

        public override DescriptorCategory Category => DescriptorCategory.Object;

        protected override List<IMemberDescriptor> PrepareMembers()
        {
            if (Type == typeof(Type))
            {
                return EmptyMembers;
            }

            // Add all public properties with a readable get method
            var memberList = (from propertyInfo in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                              where
                                  propertyInfo.CanRead && propertyInfo.GetGetMethod(false) != null &&
                                  propertyInfo.GetIndexParameters().Length == 0 &&
                                  IsMemberToVisit(propertyInfo)
                              select new PropertyDescriptor(factory.Find(propertyInfo.PropertyType), propertyInfo)
                              into member
                              where PrepareMember(member)
                              select member).Cast<IMemberDescriptor>().ToList();

            // Add all public fields
            memberList.AddRange((from fieldInfo in Type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                 where fieldInfo.IsPublic &&
                                  IsMemberToVisit(fieldInfo)
                                 select new FieldDescriptor(factory.Find(fieldInfo.FieldType), fieldInfo)
                                 into member
                                 where PrepareMember(member)
                                 select member));

            return memberList;
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
                member.Mode = (memberType != typeof(string) && memberType.IsClass) || memberType.IsInterface || Type.IsAnonymous() ? DataMemberMode.Content : DataMemberMode.Never;
            }

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
                                                            .ToFormat(memberType.FullName, member.Name, Type.FullName));
                if (!memberType.GetElementType().IsPureValueType())
                    throw new InvalidOperationException("{0} is not a pure ValueType. {1} {2} of {3} can not serialize as binary.".ToFormat(memberType.GetElementType(), memberType.FullName, member.Name, Type.FullName));
            }

            // If this member cannot be serialized, remove it from the list
            if (member.Mode == DataMemberMode.Never)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(memberAttribute?.Name))
            {
                member.Name = memberAttribute.Name;
            }

            return true;
        }
    }
}