// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Serialization
{
    class MemberSerializer
    {
        public static readonly Dictionary<string, Type> CachedTypes = new Dictionary<string, Type>();
        public static readonly Dictionary<Type, string> ReverseCachedTypes = new Dictionary<Type, string>();

        // Holds object references during serialization, useful when same object is referenced multiple time in same serialization graph.
        internal static PropertyKey<Dictionary<object, int>> ObjectSerializeReferences = new PropertyKey<Dictionary<object, int>>("ObjectSerializeReferences", typeof(SerializerExtensions),
            DefaultValueMetadata.Delegate(
                delegate
                {
                    return new Dictionary<object, int>(ObjectReferenceEqualityComparer.Default);
                }));
        internal static PropertyKey<List<object>> ObjectDeserializeReferences = new PropertyKey<List<object>>("ObjectDeserializeReferences", typeof(SerializerExtensions), DefaultValueMetadata.Delegate(delegate { return new List<object>(); }));

        internal static PropertyKey<Action<int, object>> ObjectDeserializeCallback = new PropertyKey<Action<int, object>>("ObjectDeserializeCallback", typeof(SerializerExtensions));

        /// <summary>
        /// Implements an equality comparer based on object reference instead of <see cref="object.Equals(object)"/>.
        /// </summary>
        private class ObjectReferenceEqualityComparer : EqualityComparer<object>
        {
            private static IEqualityComparer<object> defaultEqualityComparer;

            public new static IEqualityComparer<object> Default
            {
                get { return defaultEqualityComparer ?? (defaultEqualityComparer = new ObjectReferenceEqualityComparer()); }
            }

            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
    
    /// <summary>
    /// Helper for serializing members of a class.
    /// </summary>
    /// <typeparam name="T">The type of member to serialize.</typeparam>
    public abstract class MemberSerializer<T> : DataSerializer<T>
    {
        protected static bool isValueType = typeof(T).GetTypeInfo().IsValueType;
        
        // For now we hardcode here that Type subtypes should be ignored, but this should probably be a DataSerializerAttribute flag?
        protected static bool isSealed = typeof(T).GetTypeInfo().IsSealed || typeof(T) == typeof(Type);

        protected DataSerializer<T> dataSerializer;

        public MemberSerializer(DataSerializer<T> dataSerializer)
        {
            this.dataSerializer = dataSerializer;
        }

        public static DataSerializer<T> Create(SerializerSelector serializerSelector, bool nullable = true)
        {
            var dataSerializer = serializerSelector.GetSerializer<T>();
            if (!isValueType)
            {
                if (serializerSelector.ReuseReferences)
                    dataSerializer = typeof(T) == typeof(object) ? (DataSerializer<T>)new MemberReuseSerializerObject<T>(dataSerializer) : new MemberReuseSerializer<T>(dataSerializer);
                else if (!isSealed)
                    dataSerializer = typeof(T) == typeof(object) ? (DataSerializer<T>)new MemberNonSealedSerializerObject<T>(dataSerializer) : new MemberNonSealedSerializer<T>(dataSerializer);
                else if (nullable)
                    dataSerializer = new MemberNullableSerializer<T>(dataSerializer);
            }

            return dataSerializer;
        }
    }
}