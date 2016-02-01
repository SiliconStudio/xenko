// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    public static class TypeHelper
    {
        public static bool IsCollection(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
            {
                return false;
            }

            if (typeof(ICollection).GetTypeInfo().IsAssignableFrom(typeInfo))
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

        public static bool IsDictionary(Type type)
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