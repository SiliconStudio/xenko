// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Reflection
{
    // TODO: these methods should be compilant with collection/dictionary descriptors. Since they're used only for design-time, they should be removed from here anyway
    [Obsolete("This class will be removed in a future version")]
    public static class TypeHelper
    {
        [Obsolete("This method will be removed in a future version")]
        public static bool IsCollection([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
            {
                return false;
            }

            if (typeof(IList).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return true;
            }

            foreach(var iType in typeInfo.ImplementedInterfaces)
            {
                var iTypeInfo = iType.GetTypeInfo();
                if (iTypeInfo.IsGenericType && iTypeInfo.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    return true;
                }
            }

            return false;
        }

        [Obsolete("This method will be removed in a future version")]
        public static bool IsDictionary([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var typeInfo = type.GetTypeInfo();
            if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return true;
            }

            foreach (var iType in typeInfo.ImplementedInterfaces)
            {
                var iTypeInfo = iType.GetTypeInfo();
                if (iTypeInfo.IsGenericType && iTypeInfo.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
