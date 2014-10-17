// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// Object metadata created by user in order to inject them in the database
    /// </summary>
    public class ObjectMetadata : IObjectMetadata
    {
        /// <inheritdoc/>
        public string ObjectUrl { get; private set; }

        /// <inheritdoc/>
        public MetadataKey Key { get; private set; }

        /// <inheritdoc/>
        public object Value { get; set; }

        public ObjectMetadata(string owner, MetadataKey key)
        {
            ObjectUrl = owner;
            Key = key;
            Value = key.GetDefaultValue();
        }

        public ObjectMetadata(string owner, MetadataKey key, object value)
        {
            ObjectUrl = owner;
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// A generic object metadata created by user in order to inject them in the database. It provides a <see cref="GetValue"/> method which return the value casted to the generic type for convenience.
    /// </summary>
    public class ObjectMetadata<T> : ObjectMetadata
    {
        /// <inheritdoc/>
        public ObjectMetadata(string owner, MetadataKey key) : base(owner, key) { }

        /// <inheritdoc/>
        public ObjectMetadata(string owner, MetadataKey key, T value) : base(owner, key, value) { }

        /// <summary>
        /// Return the value of the metadata casted to the generic type
        /// </summary>
        public T GetValue()
        {
            return (T)Value;
        }
    }
}