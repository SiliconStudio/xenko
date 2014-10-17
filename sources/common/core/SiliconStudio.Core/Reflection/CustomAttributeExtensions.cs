// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    public static class CustomAttributeExtensions
    {
        public static T GetCustomAttributeEx<T>(this Assembly assembly) where T : Attribute
        {
            return (T)GetCustomAttributeEx(assembly, typeof(T));
        }

        public static Attribute GetCustomAttributeEx(this Assembly assembly, Type attributeType)
        {
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
            return Attribute.GetCustomAttribute(assembly, attributeType);
#else
            return assembly.GetCustomAttribute(attributeType);
#endif
        }

        public static IEnumerable<Attribute> GetCustomAttributesEx(this Assembly assembly, Type attributeType)
        {
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
            return Attribute.GetCustomAttributes(assembly, attributeType);
#else
            return assembly.GetCustomAttributes(attributeType);
#endif
        }

        public static IEnumerable<T> GetCustomAttributesEx<T>(this Assembly assembly) where T : Attribute
        {
            return GetCustomAttributesEx(assembly, typeof(T)).Cast<T>();
        }
    }
}