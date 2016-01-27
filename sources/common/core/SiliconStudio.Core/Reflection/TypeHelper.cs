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
            if (type.IsArray)
            {
                return false;
            }

            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return true;
            }

            var interfaces = type.GetTypeInfo().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                var typeInfo = interfaces[i].GetTypeInfo();
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDictionary(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            var interfaces = type.GetTypeInfo().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                var typeInfo = interfaces[i].GetTypeInfo();
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}