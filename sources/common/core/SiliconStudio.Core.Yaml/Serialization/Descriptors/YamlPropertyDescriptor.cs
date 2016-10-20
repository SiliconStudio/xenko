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
using System.Reflection;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml.Serialization.Descriptors
{
    /// <summary>
    /// A <see cref="IYamlMemberDescriptor"/> for a <see cref="PropertyInfo"/>
    /// </summary>
    public class YamlPropertyDescriptor : MemberDescriptorBase, IYamlMemberDescriptor
    {
        private readonly MethodInfo getMethod;
        private readonly MethodInfo setMethod;

        public YamlPropertyDescriptor(ITypeDescriptor typeDescriptor, PropertyInfo propertyInfo, StringComparer defaultNameComparer)
            : base(propertyInfo, defaultNameComparer)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            PropertyInfo = propertyInfo;
            TypeDescriptor = typeDescriptor;

            getMethod = propertyInfo.GetGetMethod(true);
            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(!IsPublic) != null)
            {
                setMethod = propertyInfo.GetSetMethod(!IsPublic);
            }
        }

        /// <summary>
        /// Gets the property information attached to this instance.
        /// </summary>
        /// <value>The property information.</value>
        public PropertyInfo PropertyInfo { get; }

        public override Type Type => PropertyInfo.PropertyType;

        public override bool HasSet => setMethod != null;

        public sealed override bool IsPublic => getMethod.IsPublic;

        public uint Mask { get; set; }

        public DataStyle Style { get; set; }

        public override object Get(object thisObject)
        {
            return getMethod.Invoke(thisObject, null);
        }

        public override void Set(object thisObject, object value)
        {
            if (HasSet)
                setMethod.Invoke(thisObject, new[] {value});
        }

        public override IEnumerable<T> GetCustomAttributes<T>(bool inherit)
        {
            return PropertyInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Property [{OriginalName}] from Type [{(PropertyInfo.DeclaringType != null ? PropertyInfo.DeclaringType.FullName : string.Empty)}]";
        }
    }
}
