// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using System.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Key of an effect parameter.
    /// </summary>
    public abstract class ParameterKey : PropertyKey
    {
        public ulong HashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKey" /> class.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        /// <param name="metadatas">The metadatas.</param>
        protected ParameterKey(Type propertyType, string name, int length, params PropertyKeyMetadata[] metadatas)
            : base(name, propertyType, null, metadatas)
        {
            Length = length;
            // Cache hashCode for faster lookup (string is immutable)
            // TODO: Make it unique (global dictionary?)
            UpdateName();
        }

        [DataMemberIgnore]
        public new ParameterKeyValueMetadata DefaultValueMetadata { get; private set; }

        /// <summary>
        /// Gets the number of elements for this key.
        /// </summary>
        public int Length { get; private set; }

        public abstract int Size { get; }

        internal void SetName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME || SILICONSTUDIO_RUNTIME_CORECLR
            Name = name;
#else
            Name = string.Intern(name);
#endif
            UpdateName();
        }

        internal void SetOwnerType(Type ownerType)
        {
            OwnerType = ownerType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            //return ReferenceEquals(this, obj);
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var against = obj as ParameterKey;
            if (against == null) return false;
            return (Equals(against.Name, Name));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (int)HashCode;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(ParameterKey left, ParameterKey right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(ParameterKey left, ParameterKey right)
        {
            return !Equals(left, right);
        }

        //public abstract ParameterKey AppendKeyOverride(object obj);

        public virtual ParameterKey CloneLength(int length)
        {
            throw new InvalidOperationException();
        }

        private unsafe void UpdateName()
        {
            fixed (char* bufferStart = Name)
            {
                var objectIdBuilder = new ObjectIdBuilder();
                objectIdBuilder.Write((byte*)bufferStart, sizeof(char) * Name.Length);

                var objId = objectIdBuilder.ComputeHash();
                var objIdData = (ulong*)&objId;
                HashCode = objIdData[0] ^ objIdData[1];
            }
        }

        /// <summary>
        /// Converts the value passed by parameter to the expecting value of this parameter key (for example, if value is
        /// an integer while this parameter key is expecting a float)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object ConvertValue(object value)
        {
            // If not a value type, return the value as-is
            if (!PropertyType.GetTypeInfo().IsValueType)
            {
                return value;
            }

            if (value != null)
            {
                // If target type is same type, then return the value directly
                if (PropertyType == value.GetType())
                {
                    return value;
                }

                if (PropertyType.GetTypeInfo().IsEnum)
                {
                    value = Enum.Parse(PropertyType, value.ToString());
                }
            }

            // Convert the value to the target type if different
            value = Convert.ChangeType(value, PropertyType);
            return value;
        }

        protected override void SetupMetadata(PropertyKeyMetadata metadata)
        {
            if (metadata is ParameterKeyValueMetadata)
            {
                DefaultValueMetadata = (ParameterKeyValueMetadata)metadata;
            }
            else
            {
                base.SetupMetadata(metadata);
            }
        }

        internal abstract ParameterCollection.InternalValue CreateInternalValue();
    }

    /// <summary>
    /// Key of an gereric effect parameter.
    /// </summary>
    /// <typeparam name="T">Type of the parameter key.</typeparam>
    [DataSerializer(typeof(ParameterKeySerializer<>), Mode = DataSerializerGenericMode.GenericArguments)]
    public sealed class ParameterKey<T> : ParameterKey
    {
        private static bool isValueType = typeof(T).GetTypeInfo().IsValueType;
        private static bool isValueArrayType = typeof(T).GetTypeInfo().IsArray && typeof(T).GetElementType().GetTypeInfo().IsValueType;
        private static Type internalValueArrayType = isValueArrayType ? typeof(ParameterCollection.InternalValueArray<>).MakeGenericType(typeof(T).GetElementType()) : null;

        public override bool IsValueType
        {
            get { return isValueType; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        /// <param name="metadata">The metadata.</param>
        public ParameterKey(string name, int length, PropertyKeyMetadata metadata)
            : this(name, length, new []{ metadata })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        /// <param name="metadatas">The metadatas.</param>
        public ParameterKey(string name, int length = 1, params PropertyKeyMetadata[] metadatas)
            : base(typeof(T), name, length, metadatas.Length > 0 ? metadatas : new PropertyKeyMetadata[]{ new ParameterKeyValueMetadata<T>() })
        {
        }

        [DataMemberIgnore]
        public ParameterKeyValueMetadata<T> DefaultValueMetadataT { get; private set; }

        public override int Size => Interop.SizeOf<T>();

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        public override ParameterKey CloneLength(int length)
        {
            if (!typeof(T).IsArray)
                throw new InvalidOperationException("Operation not valid on ParameterKey<T> if T is not an array type.");
            var elementType = typeof(T).GetElementType();
            return new ParameterKey<T>(Name, length, new ParameterKeyValueMetadata<T>((T)(object)Array.CreateInstance(elementType, length)));
        }

        internal override ParameterCollection.InternalValue CreateInternalValue()
        {
            if (isValueType)
                return new ParameterCollection.InternalValue<T>();

            if (isValueArrayType)
            {
                // Still a slow path for arrays due to generic constraints...
                return (ParameterCollection.InternalValue)Activator.CreateInstance(internalValueArrayType, Length);
            }

            return new ParameterCollection.InternalValueBase<T>();
        }

        protected override void SetupMetadata(PropertyKeyMetadata metadata)
        {
            if (metadata is ParameterKeyValueMetadata<T>)
            {
                DefaultValueMetadataT = (ParameterKeyValueMetadata<T>)metadata;
            }
            // Run the always base as ParameterKeyValueMetadata<T> is also ParameterKeyValueMetadata used by the base
            base.SetupMetadata(metadata);
        }

        internal override PropertyContainer.ValueHolder CreateValueHolder(object value)
        {
            throw new NotImplementedException();
        }
    }
}
