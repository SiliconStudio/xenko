// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    public static class ParameterKeys
    {
        /// <summary>
        /// Creates a value key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name">The name.</param>
        /// <returns>ParameterKey{``0}.</returns>
        public static ParameterKey<T> New<T>(T defaultValue, string name = null)
        {
            if (name == null)
                name = string.Empty;

            var length = typeof(T).IsArray ? (defaultValue != null ? ((Array)(object)defaultValue).Length : -1) : 1;
            return new ParameterKey<T>(name, length, new ParameterKeyValueMetadata<T>(defaultValue));
        }


        /// <summary>
        /// Creates a value key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadata">The metadata.</param>
        /// <returns>ParameterKey{``0}.</returns>
        public static ParameterKey<T> NewWithMetas<T>(PropertyKeyMetadata metadata)
        {
            return NewWithMetas<T>(new[] { metadata });
        }

        /// <summary>
        /// Creates a value key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadatas">The metadatas.</param>
        /// <returns>ParameterKey{``0}.</returns>
        public static ParameterKey<T> NewWithMetas<T>(PropertyKeyMetadata[] metadatas)
        {
            if (metadatas.Length > 0)
            {
                var list = new List<PropertyKeyMetadata>(metadatas) { new ParameterKeyValueMetadata<T>(default(T)) };
                metadatas = list.ToArray();
            }

            return new ParameterKey<T>(string.Empty, -1, metadatas);
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

            var metadata = new ParameterKeyValueMetadata<T>(dynamicValue);
            var result = new ParameterKey<T>(name, 1, metadata);
            return result;
        }

        public static ParameterKey<T[]> NewDynamic<T>(int arraySize, ParameterDynamicValue<T[]> dynamicValue, string name = null) where T : struct
        {
            if (name == null)
                name = string.Empty;

            var metadata = new ParameterKeyValueMetadata<T[]>(dynamicValue);
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

        public static T ComposeWith<T>(this T key, string name) where T : ParameterKey
        {
            if (name == null) throw new ArgumentNullException("name");

            // TODO: cache this string builder to avoid allocation (using for example a thread local)
            var builder = new StringBuilder(key.Name.Length + 1 + name.Length);
            builder.Append(key.Name);
            builder.Append('.');
            builder.Append(name);

            return ComposeWith(key, builder);
        }

        public static T ComposeIndexer<T>(this T key, string name, int index) where T : ParameterKey
        {
            if (name == null) throw new ArgumentNullException("name");

            // TODO: cache this string builder to avoid allocation (using for example a thread local)
            var builder = new StringBuilder(key.Name.Length + 10 + name.Length);
            builder.Append(key.Name);
            builder.Append('.');
            builder.Append(name);
            builder.Append('[');
            builder.Append(index);
            builder.Append(']');

            return ComposeWith(key, builder.ToString());
        }

        private static T ComposeWith<T>(this T key, StringBuilder builder) where T : ParameterKey
        {
            if (builder == null) throw new ArgumentNullException("builder");
            var newKey = (T)FindByName(builder.ToString());
            if (newKey == null)
            {
                throw new ArgumentException("Key [{0}] must be a registered key".ToFormat(key));
            }
            return newKey;
        }

        public static ParameterKey FindByName(string name)
        {
            // name must be XXX.YYY{.ZZZ}
            // where ZZZ can be any identifiers separated by dots but we don't check this.
            lock (keyByNames)
            {
                ParameterKey key;
                keyByNames.TryGetValue(name, out key);
                if (key == null)
                {
                    // firstDot index between XXX and YYY
                    var firstDot = name.IndexOf('.');
                    if (firstDot == -1)
                        return null;

                    // dot after YYY
                    var subKeyNameIndex = name.IndexOf('.', firstDot + 1);

                    // keyName => XXX.YYY
                    string keyName = name; 
                    string subKeyName = null;
                    if (subKeyNameIndex >= 0)
                    {
                        keyName = name.Substring(0, subKeyNameIndex);
                        // subKeyName => ZZZ
                        subKeyName = name.Substring(subKeyNameIndex);
                    }

                    // It is possible this key has been appended with mixin path (i.e. Test becomes Test.mixin[0])
                    // if it was not a "stage" value
                    if (keyByNames.TryGetValue(keyName, out key) && subKeyName != null)
                    {
                        var baseParameterKeyType = key.GetType();
                        while (baseParameterKeyType.GetGenericTypeDefinition() != typeof(ParameterKey<>))
                            baseParameterKeyType = baseParameterKeyType.GetTypeInfo().BaseType;

                        // Get default value and use it for the new subkey
                        var defaultValue = key.DefaultValueMetadata.DefaultDynamicValue ?? key.DefaultValueMetadata.GetDefaultValue();

                        // Create metadata
                        var metadataParameters = defaultValue != null ? new[] { defaultValue } : new object[0]; 
                        var metadata = Activator.CreateInstance(typeof(ParameterKeyValueMetadata<>).MakeGenericType(baseParameterKeyType.GetTypeInfo().GenericTypeArguments[0]), metadataParameters);

                        var args = new[] { name, metadata };
                        if (key.GetType().GetGenericTypeDefinition() == typeof(ParameterKey<>))
                        {
                            args = new[] { name, key.Length, metadata };
                        }
                        key = (ParameterKey)Activator.CreateInstance(key.GetType(), args);

                        // Register key. Also register real name in case it was remapped.
                        keyByNames[name] = key;
                    }
                }

                return key;
            }
        }
    }
}
