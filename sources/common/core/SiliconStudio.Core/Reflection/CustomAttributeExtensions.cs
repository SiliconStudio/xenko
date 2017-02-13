// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Reflection
{
    public static class CustomAttributeExtensions
    {
        public static T GetCustomAttributeEx<T>([NotNull] this Assembly assembly) where T : Attribute
        {
            return (T)GetCustomAttributeEx(assembly, typeof(T));
        }

        public static Attribute GetCustomAttributeEx([NotNull] this Assembly assembly, [NotNull] Type attributeType)
        {
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
            return Attribute.GetCustomAttribute(assembly, attributeType);
#else
            return assembly.GetCustomAttribute(attributeType);
#endif
        }

        public static IEnumerable<Attribute> GetCustomAttributesEx([NotNull] this Assembly assembly, [NotNull] Type attributeType)
        {
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
            return Attribute.GetCustomAttributes(assembly, attributeType);
#else
            return assembly.GetCustomAttributes(attributeType);
#endif
        }

        [NotNull]
        public static IEnumerable<T> GetCustomAttributesEx<T>([NotNull] this Assembly assembly) where T : Attribute
        {
            return GetCustomAttributesEx(assembly, typeof(T)).Cast<T>();
        }
    }
}