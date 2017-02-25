using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Reflection
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, bool> AnonymousTypes = new Dictionary<Type, bool>();

        public static bool HasInterface([NotNull] this Type type, [NotNull] Type lookInterfaceType)
        {
            return type.GetInterface(lookInterfaceType) != null;
        }

        [CanBeNull]
        public static Type GetInterface([NotNull] this Type type, [NotNull] Type lookInterfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (lookInterfaceType == null)
                throw new ArgumentNullException(nameof(lookInterfaceType));

            var typeinfo = lookInterfaceType.GetTypeInfo();
            if (typeinfo.IsGenericTypeDefinition)
            {
                if (typeinfo.IsInterface)
                    foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
                        if (interfaceType.GetTypeInfo().IsGenericType
                            && interfaceType.GetGenericTypeDefinition() == lookInterfaceType)
                            return interfaceType;

                for (var t = type; t != null; t = t.GetTypeInfo().BaseType)
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
        /// Determines whether the specified type is an anonymous type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is anonymous; otherwise, <c>false</c>.</returns>
        public static bool IsAnonymous([NotNull] this Type type)
        {
            lock (AnonymousTypes)
            {
                bool isAnonymous;
                if (AnonymousTypes.TryGetValue(type, out isAnonymous))
                    return isAnonymous;

                isAnonymous = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0
                              && type.Namespace == null
                              && type.FullName.Contains("AnonymousType");

                AnonymousTypes.Add(type, isAnonymous);
                return isAnonymous;
            }
        }

        /// <summary>
        /// Return if an object is a numeric value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True if object is a numeric value.</returns>
        public static bool IsNumeric([NotNull] this Type type)
        {
            return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
                   type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(float) || type == typeof(double) || type == typeof(decimal);
        }

        /// <summary>
        /// Determines whether the specified type is nullable <see cref="Nullable{T}.Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
        public static bool IsNullable([NotNull] this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Indicates whether the specified <paramref name="type"/> is a non-primitive struct type.
        /// </summary>
        /// <param name="type">The <see cref="Type.Type"/> to be analyzed.</param>
        /// <returns><c>True</c> if the specified <paramref name="type"/> is a non-primitive struct type; otehrwise <c>False</c>.</returns>
        public static bool IsStruct([NotNull] this Type type)
        {
            return type.GetTypeInfo().IsValueType && !type.GetTypeInfo().IsPrimitive && !type.GetTypeInfo().IsEnum;
        }

        /// <summary>
        /// Casts an object to a specified numeric type.
        /// </summary>
        /// <param name="obj">Any object</param>
        /// <param name="type">Numric type</param>
        /// <returns>Numeric value or null if the object is not a numeric value.</returns>
        [NotNull]
        public static object CastToNumericType([NotNull] this Type type, object obj)
        {
            if (!type.IsNumeric())
                throw new InvalidOperationException($"{type} is not a valid numeric type");

            if (obj is decimal && type == typeof(decimal))
                return obj; // do not convert into double

            var doubleValue = Convert.ToDouble(obj);
            unchecked
            {
                if (type == typeof(sbyte))
                    return (sbyte)doubleValue;
                if (type == typeof(byte))
                    return (byte)doubleValue;
                if (type == typeof(short))
                    return (short)doubleValue;
                if (type == typeof(ushort))
                    return (ushort)doubleValue;
                if (type == typeof(int))
                    return (int)doubleValue;
                if (type == typeof(uint))
                    return (uint)doubleValue;
                if (type == typeof(long))
                    return (long)doubleValue;
                if (type == typeof(ulong))
                    return (ulong)doubleValue;
                if (type == typeof(float))
                    return (float)doubleValue;
                if (type == typeof(double))
                    return doubleValue;
                if (type == typeof(decimal))
                    return (decimal)doubleValue;
            }
            // Note: this should never happen
            throw new NotSupportedException();
        }

        /// <summary>
        /// Check if the type is a ValueType and does not contain any non ValueType members.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPureValueType(this Type type)
        {
            if (type == null)
                return false;
            if (type == typeof(IntPtr))
                return false;
            if (type.IsPrimitive)
                return true;
            if (type.IsEnum)
                return true;
            if (!type.IsValueType)
                return false;
            // struct
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsPureValueType(fieldInfo.FieldType))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Convert a collection of objects or primitives, and convert (or unbox) each element to a double.
        /// Incompatible elements are not added to the list, therefore the resulting collection Count might differ.
        /// </summary>
        /// <param name="collection">source enumerable, can be a system array of primitives, or a collection of boxed numeric types</param>
        /// <returns>Converted collection</returns>
        [NotNull]
        public static List<double> ToListOfDoubles([NotNull] this IList collection)
        {
            var result = new List<double>(collection.Count);
            foreach (var v in collection)
            {
                if (v.GetType().IsPrimitive)
                    result.Add((double)v);
                else
                {
                    var unboxed = (double)typeof(double).CastToNumericType(v);
                    if (!double.IsNaN(unboxed))
                        result.Add(unboxed);
                }
            }
            return result;
        }
    }
}
