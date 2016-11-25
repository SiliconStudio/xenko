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
using System.Reflection;
using System.Text;

namespace SiliconStudio.Core.Yaml
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the assembly qualified name of the type, but without the assembly version or public token.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The assembly qualified name of the type, but without the assembly version or public token.</returns>
        /// <exception cref="InvalidOperationException">Unable to get an assembly qualified name for type.</exception>
        /// <example>
        ///     <list type="bullet">
        ///         <item><c>typeof(string).GetShortAssemblyQualifiedName(); // System.String,mscorlib</c></item>
        ///         <item><c>typeof(string[]).GetShortAssemblyQualifiedName(); // System.String[],mscorlib</c></item>
        ///         <item><c>typeof(List&lt;string&gt;).GetShortAssemblyQualifiedName(); // System.Collection.Generics.List`1[[System.String,mscorlib]],mscorlib</c></item>
        ///     </list>
        /// </example>
        public static string GetShortAssemblyQualifiedName(this Type type)
        {
            if (type.AssemblyQualifiedName == null)
                throw new InvalidOperationException($"Unable to get an assembly qualified name for type [{type}]");

            var sb = new StringBuilder();
            DoGetShortAssemblyQualifiedName(type, sb);
            return sb.ToString();
        }

        private static void DoGetShortAssemblyQualifiedName(Type type, StringBuilder sb, bool appendAssemblyName = true)
        {
            // namespace
            sb.Append(type.Namespace).Append(".");
            // nested declaring types
            var declaringType = type.DeclaringType;
            if (declaringType != null)
            {
                var declaringTypeName = string.Empty;
                do
                {
                    declaringTypeName = declaringType.Name + "+" + declaringTypeName;
                    declaringType = declaringType.DeclaringType;
                } while (declaringType != null);
                sb.Append(declaringTypeName);
            }
            // type
            var isArray = type.IsArray;
            if (isArray)
                type = type.GetElementType();
            sb.Append(type.Name);
            // generic arguments
            if (type.IsGenericType)
            {
                sb.Append("[[");
                var genericArguments = type.GetGenericArguments();
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    if (i > 0)
                        sb.Append("],[");
                    DoGetShortAssemblyQualifiedName(genericArguments[i], sb);
                }
                sb.Append("]]");
            }
            if (isArray)
                sb.Append("[]");
            // assembly
            if (appendAssemblyName)
                sb.Append(",").Append(GetShortAssemblyName(type.Assembly));
        }

        /// <summary>
        /// Gets the qualified name of the assembly, but without the assembly version or public token.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The qualified name of the assembly, but without the assembly version or public token.</returns>
        private static string GetShortAssemblyName(this Assembly assembly)
        {
            var assemblyName = assembly.FullName;
            var indexAfterAssembly = assemblyName.IndexOf(',');
            if (indexAfterAssembly >= 0)
            {
                assemblyName = assemblyName.Substring(0, indexAfterAssembly);
            }
            return assemblyName;
        }
    }
}
