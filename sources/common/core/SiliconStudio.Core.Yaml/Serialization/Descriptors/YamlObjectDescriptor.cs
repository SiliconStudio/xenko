// Copyright (c) 2015 SharpYaml - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// -------------------------------------------------------------------------------
// SharpYaml is a fork of YamlDotNet https://github.com/aaubry/YamlDotNet
// published with the following license:
// -------------------------------------------------------------------------------
// 
// Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Reflection;
using PropertyDescriptor = SiliconStudio.Core.Reflection.PropertyDescriptor;

namespace SiliconStudio.Core.Yaml.Serialization.Descriptors
{
    /// <summary>
    /// Default implementation of a <see cref="IYamlTypeDescriptor"/>.
    /// </summary>
    public class YamlObjectDescriptor : ObjectDescriptorBase, IYamlTypeDescriptor
    {
        public static readonly Func<object, bool> ShouldSerializeDefault = o => true;

        private static readonly object[] EmptyObjectArray = new object[0];
        private readonly ITypeDescriptorFactory factory;
        private readonly bool emitDefaultValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlObjectDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The type descriptor factory.</param>
        /// <param name="type">The type.</param>
        /// <param name="emitDefaultValues">if set to <c>true</c> [emit default values].</param>
        /// <param name="namingConvention">The naming convention.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="YamlException">type</exception>
        public YamlObjectDescriptor(ITypeDescriptorFactory factory, Type type, bool emitDefaultValues, IMemberNamingConvention namingConvention)
            : base(factory?.AttributeRegistry, type, emitDefaultValues, namingConvention)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (namingConvention == null)
                throw new ArgumentNullException(nameof(namingConvention));

            this.factory = factory;
            this.emitDefaultValues = emitDefaultValues;

            Attributes = AttributeRegistry.GetAttributes(type);

            Style = DataStyle.Any;
            foreach (var attribute in Attributes)
            {
                var styleAttribute = attribute as DataStyleAttribute;
                if (styleAttribute != null)
                {
                    Style = styleAttribute.Style;
                }
            }
        }

        /// <summary>
        /// Gets attributes attached to this type.
        /// </summary>
        public List<Attribute> Attributes { get; }

        public DataStyle Style { get; }

        protected override List<IMemberDescriptor> PrepareMembers()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (Category == DescriptorCategory.Object)
                bindingFlags |= BindingFlags.NonPublic;

            // Add all public properties with a readable get method
            var memberList = (from propertyInfo in Type.GetProperties(bindingFlags)
                where propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0 && IsMemberToVisit(propertyInfo)
                select new PropertyDescriptor(factory.Find(propertyInfo.PropertyType), propertyInfo, NamingConvention.Comparer)
                into member
                where PrepareMember(member)
                select member).Cast<IMemberDescriptor>().ToList();

            // Add all public fields
            memberList.AddRange(from fieldInfo in Type.GetFields(bindingFlags)
                where fieldInfo.IsPublic && IsMemberToVisit(fieldInfo)
                select new FieldDescriptor(factory.Find(fieldInfo.FieldType), fieldInfo, NamingConvention.Comparer)
                into member
                where PrepareMember(member)
                select member);

            // Allow to add dynamic members per type
            (AttributeRegistry as YamlAttributeRegistry)?.PrepareMembersCallback?.Invoke(this, memberList);

            return memberList;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
