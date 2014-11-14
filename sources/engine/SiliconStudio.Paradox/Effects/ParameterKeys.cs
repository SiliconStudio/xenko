// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Paradox.Effects
{
    public static class ParameterKeys
    {
        /// <summary>
        /// Creates a value key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ParameterKey<T> New<T>(T defaultValue, string name = null)
        {
            if (name == null)
                name = string.Empty;

            var length = typeof(T).IsArray ? (defaultValue != null ? ((Array)(object)defaultValue).Length : -1) : 1;
            var metadata = new ParameterKeyMetadata<T>(defaultValue);
            var result = new ParameterKey<T>(name, length, metadata);
            return result;
        }

        public static ParameterKey<T> New<T>()
        {
            return New<T>(default(T));
        }

        internal static ParameterKey<T> New<T>(string name)
        {
            return New<T>(default(T), name);
        }

        public static ParameterKey<T> NewDynamic<T>(ParameterDynamicValue<T> dynamicValue, string name = null)
        {
            if (name == null)
                name = string.Empty;

            var metadata = new ParameterKeyMetadata<T>(dynamicValue);
            var result = new ParameterKey<T>(name, 1, metadata);
            return result;
        }

        public static ParameterKey<T[]> NewDynamic<T>(int arraySize, ParameterDynamicValue<T[]> dynamicValue, string name = null) where T : struct
        {
            if (name == null)
                name = string.Empty;

            var metadata = new ParameterKeyMetadata<T[]>(dynamicValue);
            var result = new ParameterKey<T[]>(name, arraySize, metadata);
            return result;
        }
        
        /// <summary>
        /// Creates the key with specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static ParameterKey<T> IndexedKey<T>(ParameterKey<T> key, int index)
        {
            if (index == 0)
                return key;

            string keyName;

            if (key.Name[key.Name.Length - 1] == '0')
            {
                keyName = key.Name.Substring(0, key.Name.Length - 1) + index;
            }
            else
            {
                keyName = key.Name + index;
            }

            return New<T>(default(T), keyName);
        }

        private static Dictionary<string, ParameterKey> keyByNames = new Dictionary<string,ParameterKey>();

        public static ParameterKey Merge(ParameterKey key, Type ownerType, string name)
        {
            lock (keyByNames)
            {
                /*if (keyByNames.TryGetValue(name, out duplicateKey))
                {
                    if (duplicateKey.PropertyType != key.PropertyType)
                    {
                        // TODO: For now, throw an exception, but we should be nicer about it
                        // (log and allow the two keys to co-exist peacefully?)
                        throw new InvalidOperationException("Two ParameterKey with same name but different types have been initialized.");
                    }
                    return key;
                }*/

                if (string.IsNullOrEmpty(key.Name))
                    key.SetName(name);
                keyByNames[name] = key;
                
                if (key.OwnerType == null && ownerType != null)
                    key.SetOwnerType(ownerType);
            }
            return key;
        }

        public static T AppendKey<T>(this T key, object name) where T : ParameterKey
        {
            return (T)FindByName(key.Name + name);
        }

        public static ParameterKey FindByName(string name)
        {
            lock (keyByNames)
            {
                ParameterKey key;
                keyByNames.TryGetValue(name, out key);
                if (key == null)
                {
                    var firstDot = name.IndexOf('.');
                    if (firstDot == -1)
                        return null;

                    var subKeyNameIndex = name.IndexOfAny(new[] { '.', '[' }, firstDot + 1);
                    if (subKeyNameIndex == -1)
                        subKeyNameIndex = name.Length;

                    // Ignore digits at ending
                    while (subKeyNameIndex > 0 && char.IsDigit(name, subKeyNameIndex - 1))
                        subKeyNameIndex--;

                    if (subKeyNameIndex != name.Length)
                    {
                        string keyName = name.Substring(0, subKeyNameIndex);
                        string subKeyName = subKeyNameIndex != -1 ? name.Substring(subKeyNameIndex) : null;

                        // It is possible this key has been appended with mixin path (i.e. Test becomes Test.mixin[0])
                        // if it was not a "stage" value
                        if (keyByNames.TryGetValue(keyName, out key) && subKeyName != "0")
                        {
                            var realName = key.Name != keyName ? key.Name + subKeyName : name;

                            var baseParameterKeyType = key.GetType();
                            while (baseParameterKeyType.GetGenericTypeDefinition() != typeof(ParameterKey<>))
                                baseParameterKeyType = baseParameterKeyType.GetTypeInfo().BaseType;

                            // Get default value and use it for the new subkey
                            var defaultValue = key.DefaultMetadata.DefaultDynamicValue ?? key.DefaultMetadata.GetDefaultValue();

                            // Create metadata
                            var metadataParameters = defaultValue != null ? new[] { defaultValue } : new object[0]; 
                            var metadata = Activator.CreateInstance(typeof(ParameterKeyMetadata<>).MakeGenericType(baseParameterKeyType.GetTypeInfo().GenericTypeArguments[0]), metadataParameters);

                            var args = new[] { name, metadata };
                            if (key.GetType().GetGenericTypeDefinition() == typeof(ParameterKey<>))
                            {
                                args = new[] { name, key.Length, metadata };
                            }
                            key = (ParameterKey)Activator.CreateInstance(key.GetType(), args);

                            // Register key. Also register real name in case it was remapped.
                            keyByNames[name] = key;
                            if (name != realName)
                                keyByNames[realName] = key;
                        }
                    }
                }

                return key;
            }
        }
    }
}
