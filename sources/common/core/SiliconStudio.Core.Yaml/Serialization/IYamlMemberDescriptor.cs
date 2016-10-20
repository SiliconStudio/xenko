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
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml.Serialization
{
    /// <summary>
    /// Describe a member of an object.
    /// </summary>
    public interface IYamlMemberDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string OriginalName { get; }

        /// <summary>
        /// Gets the default name comparer.
        /// </summary>
        /// <value>The default name comparer.</value>
        StringComparer DefaultNameComparer { get; }

        /// <summary>
        /// Gets a value indicating whether this member is public.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this member is public; otherwise, <c>false</c>.
        /// </value>
        bool IsPublic { get; }

        /// <summary>
        /// Gets the serialization mask, that will be checked against the context to know if this field needs to be serialized.
        /// </summary>
        /// <value>
        /// The mask.
        /// </value>
        uint Mask { get; }

        /// <summary>
        /// Gets the default style attached to this member.
        /// </summary>
        /// <value>The style.</value>
        DataStyle Style { get; }

        /// <summary>
        /// Gets a value indicating whether this member should be serialized.
        /// </summary>
        /// <value><c>true</c> if [should serialize]; otherwise, <c>false</c>.</value>
        Func<object, bool> ShouldSerialize { get; }

        /// <summary>
        /// Gets the alternative names that will map back to this member (may be null).
        /// </summary>
        /// <value>The alternative names that will map back to this member (may be null).</value>
        List<string> AlternativeNames { get; }

        /// <summary>
        /// Gets or sets a custom tag to associate with this object.
        /// </summary>
        /// <value>A custom tag object.</value>
        object Tag { get; set; }

    }
}
