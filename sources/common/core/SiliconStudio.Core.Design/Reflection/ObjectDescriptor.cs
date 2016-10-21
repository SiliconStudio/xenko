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
using SiliconStudio.Core.Yaml.Serialization;

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
        public ObjectDescriptor(ITypeDescriptorFactory factory, Type type, bool emitDefaultValues, IMemberNamingConvention namingConvention)
            : base(factory?.AttributeRegistry, type, emitDefaultValues, namingConvention)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (type == null) throw new ArgumentNullException(nameof(type));
            this.factory = factory;
        }

        protected override List<IMemberDescriptor> PrepareMembers()
        {
            if (Type == typeof(Type))
            {
                return EmptyMembers;
            }

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            // TODO: we might want an option to disable non-public.
            if (Category == DescriptorCategory.Object)
                bindingFlags |= BindingFlags.NonPublic;

            // Add all public properties with a readable get method
            var memberList = (from propertyInfo in Type.GetProperties(bindingFlags)
                              where
                                  propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0 &&
                                  IsMemberToVisit(propertyInfo)
                              select new PropertyDescriptor(factory.Find(propertyInfo.PropertyType), propertyInfo, StringComparer.Ordinal)
                              into member
                              where PrepareMember(member)
                              select member).Cast<IMemberDescriptor>().ToList();

            // Add all public fields
            memberList.AddRange((from fieldInfo in Type.GetFields(bindingFlags)
                                 where fieldInfo.IsPublic &&
                                  IsMemberToVisit(fieldInfo)
                                 select new FieldDescriptor(factory.Find(fieldInfo.FieldType), fieldInfo, StringComparer.Ordinal)
                                 into member
                                 where PrepareMember(member)
                                 select member));

            return memberList;
        }
    }
}
