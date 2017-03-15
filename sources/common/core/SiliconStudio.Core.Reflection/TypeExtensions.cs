using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Reflection
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, bool> AnonymousTypes = new Dictionary<Type, bool>();

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
            if (typeinfo.IsGenericTypeDefinition)
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
        /// Determines whether the specified type is an anonymous type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is anonymous; otherwise, <c>false</c>.</returns>
        public static bool IsAnonymous(this Type type)
        {
            if (type == null)
                return false;

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
        public static bool IsNumeric(this Type type)
        {
            return type != null && (type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
                                    type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong) ||
                                    type == typeof(float) || type == typeof(double) || type == typeof(decimal));
        }

        /// <summary>
        /// Determines whether the specified type is a <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
        public static bool IsNullable(this Type type)
        {
            return type != null && Nullable.GetUnderlyingType(type) != null;
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
    }
}
