// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Core.Reflection
{
    public static class TypeExtensions
    {
        public static Color4 GetUniqueColor(this Type type)
        {
            var hash = type.GetUniqueHash();
            var hue = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(type)?.CustomHue ?? hash % 360;
            return new ColorHSV(hue, 0.75f + (hash % 101) / 400f, 0.5f + (hash % 151) / 300f, 1).ToColor();
        }

        public static float GetUniqueHue(this Type type)
        {
            return TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(type)?.CustomHue ?? type.GetUniqueHash() % 360;
        }

        public static int GetUniqueHash(this Type type)
        {
            var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(type);
            var hash = displayAttribute?.Name.GetHashCode() ?? type.Name.GetHashCode();
            return hash >> 16 ^ hash;
        }

        /// <summary>
        /// Gets the display name of the given type. The display name is the name of the type, or, if the <see cref="DisplayAttribute"/> is
        /// applied on the type, value of the <see cref="DisplayAttribute.Name"/> property.
        /// </summary>
        /// <param name="type">The type for which to get the display name.</param>
        /// <returns>A string representing the display name of the type.</returns>
        public static string GetDisplayName(this Type type)
        {
            var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(type);
            return !string.IsNullOrEmpty(displayAttribute?.Name) ? displayAttribute.Name : type.Name;
        }
    }
}
