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

        public static bool HasInterface(this Type type, Type lookInterfaceType)
        {
            return type.GetInterface(lookInterfaceType) != null;
        }

        public static Type GetInterface(this Type type, Type lookInterfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (lookInterfaceType == null)
                throw new ArgumentNullException(nameof(lookInterfaceType));

            var typeinfo = lookInterfaceType.GetTypeInfo();
            if (typeinfo .IsGenericTypeDefinition)
            {
                if (typeinfo.IsInterface)
                    foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
                        if (interfaceType.GetTypeInfo().IsGenericType
                            && interfaceType.GetGenericTypeDefinition() == lookInterfaceType)
                            return interfaceType;

                for (Type t = type; t != null; t = t.GetTypeInfo().BaseType)
                    if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == lookInterfaceType)
                        return t;
            }
            else
            {
                if (lookInterfaceType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    return lookInterfaceType;
            }

            return null;
        }

        /// <summary>
        /// Gets the assembly qualified name of the type, but without the assembly version or public token.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The assembly qualified name of the type, but without the assembly version or public token.</returns>
        /// <exception cref="System.InvalidOperationException">Unable to get an assembly qualified name for type [{0}].DoFormat(type)</exception>
        internal static string GetShortAssemblyQualifiedName(this Type type)
        {
            var typeName = type.AssemblyQualifiedName;
            if (typeName == null)
            {
                throw new InvalidOperationException("Unable to get an assembly qualified name for type [{0}]".ToFormat(type));
            }

            var indexAfterType = typeName.IndexOf(',');
            if (indexAfterType >= 0)
            {
                var indexAfterAssembly = typeName.IndexOf(',', indexAfterType + 1);
                if (indexAfterAssembly >= 0)
                {
                    typeName = typeName.Substring(0, indexAfterAssembly).Replace(" ", string.Empty);
                }
            }
            return typeName;
        }

        /// <summary>
        /// Compare two objects to see if they are equal or not. Null is acceptable.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreEqual(object a, object b)
        {
            if (a == null)
                return b == null;
            if (b == null)
                return false;
            return a.Equals(b) || b.Equals(a);
        }

        /// <summary>
        /// Determines whether the specified type is nullable <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Indicates whether the specified <paramref name="type"/> is a non-primitive struct type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be analyzed.</param>
        /// <returns><c>True</c> if the specified <paramref name="type"/> is a non-primitive struct type; otehrwise <c>False</c>.</returns>
        public static bool IsStruct(this Type type)
        {
            return type != null && type.GetTypeInfo().IsValueType && !type.GetTypeInfo().IsPrimitive && !type.GetTypeInfo().IsEnum;
        }

        /// <summary>
        /// Gets the minimum value for the given numeric type.
        /// </summary>
        /// <param name="type">The type for which to get the minimum value.</param>
        /// <returns>The minimum value of the given type.</returns>
        /// <exception cref="ArgumentException">The given type is not a numeric type.</exception>
        /// <remarks>A type is numeric when <see cref="IsNumeric"/> returns true.</remarks>
        public static object GetMinimum(this Type type)
        {
            if (type == typeof(sbyte))
                return sbyte.MinValue;
            if (type == typeof(short))
                return short.MinValue;
            if (type == typeof(int))
                return int.MinValue;
            if (type == typeof(long))
                return long.MinValue;
            if (type == typeof(byte))
                return byte.MinValue;
            if (type == typeof(ushort))
                return ushort.MinValue;
            if (type == typeof(uint))
                return uint.MinValue;
            if (type == typeof(ulong))
                return ulong.MinValue;
            if (type == typeof(float))
                return float.MinValue;
            if (type == typeof(double))
                return double.MinValue;
            if (type == typeof(decimal))
                return decimal.MinValue;

            throw new ArgumentException("Numeric type expected");
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

        /// <summary>
        /// Gets the maximum value for the given numeric type.
        /// </summary>
        /// <param name="type">The type for which to get the maximum value.</param>
        /// <returns>The maximum value of the given type.</returns>
        /// <exception cref="ArgumentException">The given type is not a numeric type.</exception>
        /// <remarks>A type is numeric when <see cref="IsNumeric"/> returns true.</remarks>
        public static object GetMaximum(this Type type)
        {
            if (type == typeof(sbyte))
                return sbyte.MaxValue;
            if (type == typeof(short))
                return short.MaxValue;
            if (type == typeof(int))
                return int.MaxValue;
            if (type == typeof(long))
                return long.MaxValue;
            if (type == typeof(byte))
                return byte.MaxValue;
            if (type == typeof(ushort))
                return ushort.MaxValue;
            if (type == typeof(uint))
                return uint.MaxValue;
            if (type == typeof(ulong))
                return ulong.MaxValue;
            if (type == typeof(float))
                return float.MaxValue;
            if (type == typeof(double))
                return double.MaxValue;
            if (type == typeof(decimal))
                return decimal.MaxValue;

            throw new ArgumentException("Numeric type expected");
        }
    }
}
