// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A container to handle a hierarchical collection of effect variables.
    /// </summary>
    [DataSerializer(typeof(Data.ParameterCollectionSerializer))]
    [DebuggerTypeProxy(typeof(CollectionDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    [DataContract("!ParameterCollection")]
    public class ParameterCollection : IParameterCollectionInheritanceInternal, IDictionary<ParameterKey, object>
    {
        // Internal values
        internal FastListStruct<KeyValuePair<ParameterKey, InternalValue>> InternalValues;

        // Value ordered according to this.keys
        internal InternalValue[] IndexedInternalValues;

        // Updated every time InternalValues.Keys is changed
        internal int KeyVersion = 1;

        // Either a ParameterCollection (inherits everything) or an InheritanceDefinition (inherits only specific key, ability to remap them as well)
        private readonly List<IParameterCollectionInheritanceInternal> sources;
        
        // TODO: Maybe make this structure more simpler (second Dictionary is only here for event with ParameterKey == null mapping to all keys)
        private Dictionary<ValueChangedEventKey, Dictionary<ParameterKey, InternalValueChangedDelegate>> valueChangedEvents;

        // Match a specific ordering given by "keys" (especially useful for effects or components)
        private Dictionary<ParameterKey, int> keyMapping;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ParameterCollection(string name)
        {
            Name = name;
            sources = new List<IParameterCollectionInheritanceInternal>();
            InternalValues = new FastListStruct<KeyValuePair<ParameterKey, InternalValue>>(4);
        }

        // Delegate definitions
        internal delegate void OnUpdateValueDelegate(ParameterCollection source, ParameterKey key, InternalValue internalValue);
        public delegate void ValueChangedDelegate(ParameterKey key, InternalValue internalValue, object oldValue);
        internal delegate void InternalValueChangedDelegate(InternalValue internalValue, object oldValue);
        internal event OnUpdateValueDelegate OnUpdateValue;

        [DataMemberIgnore]
        public string Name { get; set; }

        /// <summary>
        /// Gets the sources for this collection.
        /// </summary>
        [DataMemberIgnore]
        public IParameterCollectionInheritance[] Sources
        {
            get { return sources.ToArray(); }
        }

        void ICollection<KeyValuePair<ParameterKey, object>>.Add(KeyValuePair<ParameterKey, object> item)
        {
            SetObject(item.Key, item.Value);
        }

        public void Clear()
        {
            // TODO: Proper clean that also propagate events to sources?
            sources.Clear();
            InternalValues.Clear();
            if (valueChangedEvents != null)
                valueChangedEvents.Clear();
            IndexedInternalValues = null;
        }

        bool ICollection<KeyValuePair<ParameterKey, object>>.Contains(KeyValuePair<ParameterKey, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<ParameterKey, object>>.CopyTo(KeyValuePair<ParameterKey, object>[] array, int arrayIndex)
        {
            var keyvalues = InternalValues.Items.Select(x => new KeyValuePair<ParameterKey, object>(x.Key, x.Value.Object)).ToList();
            keyvalues.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<ParameterKey, object>>.Remove(KeyValuePair<ParameterKey, object> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of parameters stored in this instance..
        /// </summary>
        public int Count
        {
            get { return InternalValues.Count; }
        }

        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets the keys of this collection.
        /// </summary>
        public IEnumerable<ParameterKey> Keys
        {
            get { return new KeyCollection(this); }
        }

        public ICollection<object> Values { get; private set; }

        /// <summary>
        /// Gets the number of internal values.
        /// </summary>
        internal int InternalCount
        {
            get { return InternalValues.Count; }
        }

        /// <summary>
        /// Adds an event that will be raised when a value is updated.
        /// </summary>
        /// <param name="key">Key to listen to (or null to listen to everything).</param>
        /// <param name="valueUpdated">Delegate that will be called when value changes.</param>
        public void AddEvent(ParameterKey key, ValueChangedDelegate valueUpdated)
        {
            if (valueChangedEvents == null)
                valueChangedEvents = new Dictionary<ValueChangedEventKey, Dictionary<ParameterKey, InternalValueChangedDelegate>>();

            // check if the delegate was already added
            var delegateKey = new ValueChangedEventKey(key, valueUpdated);
            if (valueChangedEvents.ContainsKey(delegateKey))
                return;

            valueChangedEvents.Add(delegateKey, new Dictionary<ParameterKey, InternalValueChangedDelegate>());

            if (key != null)
            {
                var internalValue = GetInternalValue(key);
                if (internalValue != null)
                    UpdateValueChanged(key, internalValue, null);
            }
            else
            {
                foreach (var internalValue in InternalValues)
                {
                    UpdateValueChanged(internalValue.Key, internalValue.Value, null);
                }
            }
        }

        /// <summary>
        /// Removes an event previously added with AddEvent.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueUpdated"></param>
        internal void RemoveEvent(ParameterKey key, ValueChangedDelegate valueUpdated)
        {
            if (valueChangedEvents.ContainsKey(new ValueChangedEventKey(key, valueUpdated)))
            {
                if (key != null)
                {
                    var internalValue = GetInternalValue(key);
                    if (internalValue != null)
                        UpdateValueChanged(key, null, internalValue);
                }
                else
                {
                    foreach (var internalValue in InternalValues)
                    {
                        UpdateValueChanged(internalValue.Key, null, internalValue.Value);
                    }
                }

                valueChangedEvents.Remove(new ValueChangedEventKey(key, valueUpdated));
            }
        }

        /// <summary>
        /// Determines whether the specified instance contains a parameter key.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains this key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(ParameterKey key)
        {
            int index;
            return GetKeyIndex(key, out index);
        }

        public void Add(ParameterKey key, object value)
        {
            SetObject(key, value);
        }

        bool IDictionary<ParameterKey, object>.Remove(ParameterKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(ParameterKey key, out object value)
        {
            if (key == null) throw new ArgumentNullException("key");

            int index;
            value = null;
            if (!GetKeyIndex(key, out index))
            {
                return false;
            }

            var internalValue = InternalValues.Items[index].Value;

            value = internalValue.Object;
            return true;
        }

        public object this[ParameterKey key]
        {
            get
            {
                return GetObject(key);
            }
            set
            {
                SetObject(key, value);
            }
        }

        ICollection<ParameterKey> IDictionary<ParameterKey, object>.Keys
        {
            get
            {
                return InternalValues.Items.Select(x => x.Key).ToList();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<ParameterKey, object>> GetEnumerator()
        {
            return InternalValues.Select(x => new KeyValuePair<ParameterKey, object>(x.Key, x.Value.Object)).GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("ParameterCollection [{0}]", Name);
        }

        private static void WriteParameters(StringBuilder builder, ParameterCollection parameters, int indent, bool isArray)
        {
            var indentation = "";
            for (var i = 0; i < indent - 1; ++i)
                indentation += "    ";
            var first = true;
            foreach (var usedParam in parameters)
            {
                builder.Append("@P ");
                builder.Append(indentation);
                if (isArray && first)
                {
                    builder.Append("  - ");
                    first = false;
                }
                else if (indent > 0)
                    builder.Append("    ");

                if (usedParam.Key == null)
                    builder.Append("null");
                else
                    builder.Append(usedParam.Key);
                builder.Append(": ");
                if (usedParam.Value == null)
                    builder.AppendLine("null");
                else
                {
                    if (usedParam.Value is ParameterCollection)
                    {
                        WriteParameters(builder, usedParam.Value as ParameterCollection, indent + 1, false);
                    }
                    else if (usedParam.Value is ParameterCollection[])
                    {
                        var collectionArray = (ParameterCollection[])usedParam.Value;
                        foreach (var collection in collectionArray)
                            WriteParameters(builder, collection, indent + 1, true);
                    }
                    else if (usedParam.Value is Array || usedParam.Value is IList)
                    {
                        builder.AppendLine(string.Join(", ", (IEnumerable<object>)usedParam.Value));
                    }
                    else
                    {
                        builder.AppendLine(usedParam.Value.ToString());
                    }
                }
            }
        }

        public string ToStringDetailed()
        {
            var builder = new StringBuilder();
            WriteParameters(builder, this, 0, false);
            return builder.ToString();
        }

        public void Add<T>(ParameterKey<T> key, T value)
        {
            Set(key, value);
        }


        internal int GetKeyIndex(ParameterKey key)
        {
            int index = InternalValueBinarySearch(key);

            if (index < 0)
                return -1;

            return index;
        }

        protected bool GetKeyIndex(ParameterKey key, out int index)
        {
            index = InternalValueBinarySearch(key);

            if (index >= 0)
                return true;

            index = -1;
            return false;
        }

        /// <summary>
        /// Gets the index of an InternalValue within IndexedInternalValues given its key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
#if !SILICONSTUDIO_PLATFORM_IOS
        // We can't inline on iOS temporarily due to https://bugzilla.xamarin.com/show_bug.cgi?id=17558
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected int InternalValueBinarySearch(ParameterKey key)
        {
            int start = 0;
            int end = InternalValues.Count - 1;
            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var hash1 = InternalValues.Items[middle].Key.HashCode;
                var hash2 = key.HashCode;
                
                if (hash1 == hash2)
                {
                    return middle;
                }
                if (hash1 < hash2)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }
            return ~start;
        }

        /// <summary>
        /// Gets or creates an internal value index given its key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected int GetOrCreateKeyIndex(ParameterKey key)
        {
            int index = InternalValueBinarySearch(key);

            if (index < 0)
            {
                lock (sources)
                {
                    index = ~index;
                    InternalValues.Insert(index, new KeyValuePair<ParameterKey, InternalValue>(key, null));
                    KeyVersion++;
                }
            }

            return index;
        }
        
        /// <summary>
        /// Gets the value for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The value for the specified key</returns>
        public T Get<T>(ParameterKey<T> key)
        {
            T result;
            Get(key, out result);
            return result;
        }

        /// <summary>
        /// Gets the value for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        public void Get<T>(ParameterKey<T> key, out T result)
        {
            if (key == null) throw new ArgumentNullException("key");

            int index;
            if (!GetKeyIndex(key, out index))
            {
                result = key.DefaultValueMetadataT.DefaultValue;
                return;
            }

            GetValue(InternalValues.Items[index].Value, out result);
        }

        public object GetObject(ParameterKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            int index;
            if (!GetKeyIndex(key, out index))
            {
                return key.DefaultValueMetadata.GetDefaultValue();
            }

            var internalValue = InternalValues.Items[index].Value;

            return internalValue.Object;
        }
        
        internal void GetValue<T>(InternalValue internalValue, out T result)
        {
            result = ((InternalValueBase<T>)internalValue).Value;
        }

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T TryGet<T>(ParameterKey<T> key)
        {
            T result;
            TryGet(key, out result);
            return result;
        }

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryGet<T>(ParameterKey<T> key, out T result)
        {
            if (key == null) throw new ArgumentNullException("key");

            int index;
            if (!GetKeyIndex(key, out index))
            {
                result = default(T);
                return false;
            }

            GetValue(InternalValues.Items[index].Value, out result);
            return true;
        }

        /// <summary>
        /// Sets the default value for the specified key (if undefined, otherwise do nothing).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="overrideIfInherited">Specifies if inherited value should be overriden.</param>
        public void SetDefault(ParameterKey key, bool overrideIfInherited = false)
        {
            if (key == null) throw new ArgumentNullException("key");

            bool newValue;
            var index = GetOrCreateKeyIndex(key);
            if (InternalValues.Items[index].Value != null && !overrideIfInherited)
                return;

            GetOrCreateInternalValue(index, key, out newValue);

            if (newValue && OnUpdateValue != null) OnUpdateValue(this, key, InternalValues.Items[GetKeyIndex(key)].Value);
        }

        /// <summary>
        /// Sets a struct value for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(ParameterKey<T> key, T value)
        {
            if (key == null) throw new ArgumentNullException("key");

            bool newValue;
            var index = GetOrCreateKeyIndex(key);
            var internalValue = InternalValues.Items[index].Value;
            object oldValue = (internalValue != null && internalValue.ValueChanged != null) ? internalValue.Object : null;
            internalValue = GetOrCreateInternalValue(index, key, out newValue);

            ((InternalValueBase<T>)internalValue).Value = value;
            internalValue.Counter++;

            if (newValue && OnUpdateValue != null) OnUpdateValue(this, key, internalValue);

            if (internalValue.ValueChanged != null)
                internalValue.ValueChanged(internalValue, oldValue);
        }

        /// <summary>
        /// Sets an array of valuetypes for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The array of valuetypes.</param>
        public void SetArray<T>(ParameterKey<T[]> key, T[] values) where T : struct
        {
            Set(key, values, 0, values.Length);
        }

        /// <summary>
        /// Sets an array of valuetypes for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The array of valuetypes.</param>
        /// <param name="destinationOffset">The destination offset.</param>
        /// <param name="count">The number of elements to copy from value array.</param>
        public void Set<T>(ParameterKey<T[]> key, T[] values, int destinationOffset, int count) where T : struct
        {
            if (key == null) throw new ArgumentNullException("key");

            bool newValue;
            var index = GetOrCreateKeyIndex(key);
            var internalValue = (InternalValueArray<T>)InternalValues.Items[index].Value;
            object oldValue = (internalValue != null && internalValue.ValueChanged != null) ? internalValue.Object : null;
            internalValue = (InternalValueArray<T>)GetOrCreateInternalValue(index, key, out newValue);

            // First use with a null default value? (happen with variable size array)
            if (internalValue.Value == null || internalValue.Value.Length < count)
            {
                if (destinationOffset != 0)
                    throw new InvalidOperationException("Should use destinationOffset 0 and real count for first set if no default value.");
                internalValue.Value = new T[count];
            }

            for (int i = 0; i < count; ++i)
                ((InternalValueArray<T>)internalValue).Value[destinationOffset + i] = values[i];
            internalValue.Counter++;

            if (newValue && OnUpdateValue != null) OnUpdateValue(this, key, internalValue);

            if (internalValue.ValueChanged != null)
                internalValue.ValueChanged(internalValue, oldValue);
        }

        /// <summary>
        /// Sets an array of valuetypes for the specified key.
        /// </summary>
        /// <typeparam name="T">A valuetype</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The array of valuetypes.</param>
        /// <param name="sourceOffset">The source offset.</param>
        /// <param name="destinationOffset">The destination offset.</param>
        /// <param name="count">The number of elements to copy from value array.</param>
        public void Set<T>(ParameterKey<T[]> key, T[] values, int sourceOffset, int destinationOffset, int count) where T : struct
        {
            if (key == null) throw new ArgumentNullException("key");

            bool newValue;
            var index = GetOrCreateKeyIndex(key);
            var internalValue = (InternalValueArray<T>)InternalValues.Items[index].Value;
            object oldValue = (internalValue != null && internalValue.ValueChanged != null) ? internalValue.Object : null;
            internalValue = (InternalValueArray<T>)GetOrCreateInternalValue(index, key, out newValue);

            // First use with a null default value? (happen with variable size array)
            if (internalValue.Value == null || internalValue.Value.Length < count)
            {
                if (destinationOffset != 0)
                    throw new InvalidOperationException("Should use destinationOffset 0 and real count for first set if no default value.");
                internalValue.Value = new T[count];
            }

            for (int i = 0; i < count; ++i)
                ((InternalValueArray<T>)internalValue).Value[destinationOffset + i] = values[sourceOffset + i];
            internalValue.Counter++;

            if (newValue && OnUpdateValue != null) OnUpdateValue(this, key, internalValue);

            if (internalValue.ValueChanged != null)
                internalValue.ValueChanged(internalValue, oldValue);
        }

        public void SetObject(ParameterKey key, object resourceValue)
        {
            if (key == null) throw new ArgumentNullException("key");

            bool newValue;
            var index = GetOrCreateKeyIndex(key);
            var internalValue = InternalValues.Items[index].Value;
            object oldValue = (internalValue != null && internalValue.ValueChanged != null) ? internalValue.Object : null;
            internalValue = GetOrCreateInternalValue(index, key, out newValue);

            internalValue.Object = resourceValue;
            internalValue.Counter++;

            if (newValue && OnUpdateValue != null) OnUpdateValue(this, key, internalValue);

            if (internalValue.ValueChanged != null)
                internalValue.ValueChanged(internalValue, oldValue);
        }

        /// <summary>
        /// Removes the specified key and associated value
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="InvalidOperationException">If trying to remove a key from a collection that is not the owner. Or trying to remove a key that is referenced by a dynamic key</exception>
        public void Remove(ParameterKey key)
        {
            lock (sources)
            {
                int index = GetKeyIndex(key); //mapKeyToIndex[key]);
                if (index < 0) return;
                var internalValue = InternalValues.Items[index].Value;
                ReleaseValue(InternalValues.Items[index].Key, InternalValues.Items[index].Value);
                InternalValues.Items[index] = new KeyValuePair<ParameterKey, InternalValue>(key, null);
                //mapKeyToIndex.Remove(key);
                InternalValues.RemoveAt(index);
                KeyVersion++;
                //IndexedInternalValues = InternalValues.Items;
                OnKeyUpdate(key, null, internalValue);

                // TODO: Should try to inherit this value from another collection (if present)

                if (OnUpdateValue != null) OnUpdateValue(this, key, null);
            }
        }

        /// <summary>
        /// Tests if the values in parameters are contained into this instance.
        /// It will automatically handle default values as well (that is, if something is set in parameters with default value but not set in this instance, it will be ignored).
        /// </summary>
        /// <param name="parameters">The collection of parameters that should be included in this one.</param>
        /// <returns>True if this collection contains all values from parameters. False otherwise.</returns>
        public bool Contains(ParameterCollection parameters)
        {
            // TODO: Possible optimization: iterate on both sorted collections?
            foreach (var internalValue2 in parameters.InternalValues)
            {
                var internalValue1 = GetInternalValue(internalValue2.Key);

                // If parameter is not in parameter 1, we still allow it if it was a default value
                // Defer actual test to InternalValue override (avoid boxing)
                if (internalValue1 == null && internalValue2.Value.IsDefaultValue(internalValue2.Key))
                    continue;

                // Otherwise, values must match
                // Defer actual test to InternalValue override (avoid boxing)
                if (!internalValue2.Value.ValueEquals(internalValue1))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds the sources.
        /// </summary>
        /// <param name="parameterCollections">The effect variable collections.</param>
        public void AddSources(params IParameterCollectionInheritance[] newSources)
        {
            //if (OnUpdateValue != null)
            //    throw new NotSupportedException("Adding sources to ParameterCollection which are inherited is not supported yet.");

            // TODO: Check for multiple inheritance

            var oldSources = sources.ToArray();
            foreach (IParameterCollectionInheritanceInternal source in newSources)
            {
                if (sources.Contains(source))
                    continue;

                // Add the new source collection
                sources.Add(source);
            }

            UpdateSources(oldSources);

            if (OnUpdateValue != null) OnUpdateValue(this, null, null);

            lock (sources)
            {
                // Iterate on new hierarchy
                for (int i = oldSources.Length; i < sources.Count; ++i)
                {
                    InternalValues.EnsureCapacity(sources[i].GetInternalValueCount());

                    // Iterate on each keys
                    foreach (var sourceInternalValue in sources[i].GetInternalValues())
                    {
                        var key = sourceInternalValue.Key;

                        var localIndex = GetKeyIndex(key);

                        if (localIndex != -1)
                        {
                            if (FindOverrideGroupIndex(InternalValues.Items[localIndex]) >= FindOverrideGroupIndex(sourceInternalValue))
                                continue;
                        }

                        InheritValue(sourceInternalValue.Value, key);
                        localIndex = GetKeyIndex(key);
                        if (OnUpdateValue != null) OnUpdateValue(this, key, InternalValues.Items[localIndex].Value);
                    }
                }
                KeyVersion++;
            }
        }

        /// <summary>
        /// Removes the sources.
        /// </summary>
        /// <param name="parameterCollection">The source parameter collection.</param>
        public bool RemoveSource(IParameterCollectionInheritance removedInheritance)
        {
            var oldSources = sources.ToArray();

            var internalValueSources = InternalValues.Select(x =>
                {
                    var sourceIndex = FindOverrideGroupIndex(x);
                    return sourceIndex == sources.Count ? null : sources[sourceIndex];
                }).ToArray();

            if (!sources.Remove((IParameterCollectionInheritanceInternal)removedInheritance))
                return false;

            UpdateSources(oldSources);
            if (OnUpdateValue != null) OnUpdateValue(this, null, null);

            var removedSources = oldSources.Except(sources).ToArray();

            lock (sources)
            {
                for (int index = 0, index2 = 0; index < this.InternalCount; ++index, ++index2)
                {
                    var internalValue = InternalValues[index];
                    var key = internalValue.Key;
                    var source = internalValueSources[index2];
                    if (source != null && removedSources.Contains(source))
                    {
                        // TODO: Inherit from another value (if any)
                        InternalValues.RemoveAt(index--);
                        if (OnUpdateValue != null) OnUpdateValue(this, key, null);
                        OnKeyUpdate(key, null, internalValue.Value);
                    }
                }
                KeyVersion++;
            }

            return true;
        }

        /// <summary>
        /// Copy a shared value from this instance to another instance.
        /// </summary>
        /// <param name="fromKey">From key.</param>
        /// <param name="toKey">To key.</param>
        /// <param name="toCollection">To collection.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fromKey
        /// or
        /// toCollection
        /// </exception>
        /// <exception cref="System.InvalidOperationException">CopyingReadOnly is not supporting Sources from origin</exception>
        public void CopySharedTo(ParameterKey fromKey, ParameterKey toKey, ParameterCollection toCollection)
        {
            if (fromKey == null) throw new ArgumentNullException("fromKey");
            if (toCollection == null) throw new ArgumentNullException("toCollection");
            if (sources.Count > 0)
            {
                throw new InvalidOperationException("CopyingReadOnly is not supporting Sources from origin");
            }

            toKey = toKey ?? fromKey;

            var index = GetKeyIndex(fromKey);
            if (index == -1)
            {
                return;
            }

            var internalValue = InternalValues.Items[index];

            var toIndex = toCollection.GetOrCreateKeyIndex(toKey);
            if (UpdateInternalValue(ref toCollection.InternalValues.Items[toIndex], toKey, internalValue.Value))
            {
                // TODO: Temporarely: increase Keyversion so that ParameterCollectionGroup.Update is not exiting on needUpdate = false. Changing internal values in this case is like changing a key
                toCollection.KeyVersion++;
            }
        }

        private bool UpdateInternalValue(ref KeyValuePair<ParameterKey, InternalValue> keyValue, ParameterKey toKey, InternalValue newValue)
        {
            var previousValue = keyValue.Value;
            keyValue = new KeyValuePair<ParameterKey, InternalValue>(toKey, newValue);
            return !ReferenceEquals(previousValue, newValue);
        }

        /// <summary>
        /// Removes the value locally and try to get a value from a source.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Reset(ParameterKey key)
        {
            int index = GetKeyIndex(key);
            if (index == -1)
                return;

            // If overriden in a source, inherits it
            for (int i = sources.Count - 1; i >= 0; --i)
            {
                var source = sources[i];
                var internalValue = source.GetInternalValue(key);
                if (internalValue != null)
                {
                    InheritValue(internalValue, key);
                    return;
                }
            }

            // Otherwise, simply remove it
            var oldInternalValue = InternalValues.Items[index].Value;
            InternalValues.RemoveAt(index);
            KeyVersion++;

            // Notify InternalValue change
            OnKeyUpdate(key, null, oldInternalValue);
        }

        /// <summary>
        /// Determines whether [is value owner] of [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if [is value owner] of [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValueOwner(InternalValue internalValue)
        {
            if (internalValue != null)
                return internalValue.Owner == this;
            return false;
        }

        /// <inheritdoc/>
        protected void Destroy()
        {
            //base.Destroy();

            if (OnUpdateValue != null)
                throw new InvalidOperationException("Cannot dispose a parameter collection that is used as a source.");

            // Unsubscribes from all sources
            for (int i = 0; i < sources.Count; ++i)
            {
                var parameterCollection = sources[i].GetParameterCollection();
                var updateValueDelegate = sources[i].GetUpdateValueDelegate(effectVariableCollection_OnUpdateValue);
                parameterCollection.OnUpdateValue -= updateValueDelegate;
            }

            for (int i = 0; i < InternalCount; i++)
            {
                ReleaseValue(InternalValues.Items[i].Key, InternalValues.Items[i].Value);
            }
        }

        /// <summary>
        /// Create an internal value given its ParameterKey.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static InternalValue CreateInternalValue(ParameterKey key)
        {
            return key.CreateInternalValue();
        }
        
        private object GetResource(InternalValue internalValue)
        {
            return internalValue.Object;
        }
        
        private void OnKeyUpdate(ParameterKey key, InternalValue internalValue, InternalValue oldValue)
        {
            UpdateValueChanged(key, internalValue, oldValue);
            if (keyMapping != null)
            {
                UpdateKeyMapping(key, internalValue);
            }
        }

        /// <summary>
        /// Inherits an InternalValue.
        /// </summary>
        /// <param name="internalValue"></param>
        /// <param name="key"></param>
        private void InheritValue(InternalValue internalValue, ParameterKey key)
        {
            int index = GetKeyIndex(key);
            var oldInternalValue = index != -1 ? InternalValues.Items[index].Value : null;

            // Copy the InternalValue in this ParameterCollection
            index = GetOrCreateKeyIndex(key);
            InternalValues.Items[index] = new KeyValuePair<ParameterKey, InternalValue>(key, internalValue);

            // Notify InternalValue change
            OnKeyUpdate(key, internalValue, oldInternalValue);
        }
        
        /// <summary>
        /// Updates OnUpdateValue delegate for newly added/removed sources.
        /// </summary>
        /// <param name="oldSources"></param>
        private void UpdateSources(IParameterCollectionInheritance[] oldSources)
        {
            foreach (IParameterCollectionInheritanceInternal source in oldSources)
            {
                if (sources.Contains(source))
                    continue;
                var parameterCollection = source.GetParameterCollection();
                var updateValueDelegate = source.GetUpdateValueDelegate(effectVariableCollection_OnUpdateValue);
                parameterCollection.OnUpdateValue -= updateValueDelegate;
            }

            foreach (IParameterCollectionInheritanceInternal source in sources)
            {
                if (oldSources.Contains(source))
                    continue;
                var parameterCollection = source.GetParameterCollection();
                var updateValueDelegate = source.GetUpdateValueDelegate(effectVariableCollection_OnUpdateValue);
                parameterCollection.OnUpdateValue += updateValueDelegate;
            }
        }

        /// <summary>
        /// Returns index in flattened hierarchy if positive, otherwise sources.Count (this) or -1 (not found).
        /// </summary>
        /// <param name="internalValue"></param>
        /// <returns></returns>
        private int FindOverrideGroupIndex(KeyValuePair<ParameterKey, InternalValue> internalValue)
        {
            if (internalValue.Value.Owner == this)
                return sources.Count;

            // Fast lookup
            if (internalValue.Value.Owner != null)
            {
                int flattenedIndex = sources.IndexOf(internalValue.Value.Owner);
                if (flattenedIndex != -1)
                    return flattenedIndex;
            }

            // Otherwise check values
            for (int i = sources.Count - 1; i >= 0; --i)
            {
                var source = sources[i];
                if (source.GetInternalValues().Any(x => x.Value == internalValue.Value))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Called when an InternalValue has been updated.
        /// It will recursively notify dependents ParameterCollection as well.
        /// </summary>
        private void effectVariableCollection_OnUpdateValue(ParameterCollection source, ParameterKey key, InternalValue sourceInternalValue)
        {
            effectVariableCollection_OnUpdateValueLocal(source, key, sourceInternalValue);
            if (OnUpdateValue != null)
                OnUpdateValue(this, key, sourceInternalValue);
        }

        private void effectVariableCollection_OnUpdateValueLocal(ParameterCollection source, ParameterKey key, InternalValue sourceInternalValue)
        {
            // Sources changed
            if (key == null)
            {
                return;
            }

            var sourceIndex = sources.IndexOf(source);

            if (sourceInternalValue == null)
            {
                int currentIndex = GetKeyIndex(key);
                if (currentIndex == -1)
                    return;

                var currentSourceIndex = FindOverrideGroupIndex(InternalValues.Items[currentIndex]);

                if (currentSourceIndex > sourceIndex && currentSourceIndex != -1)
                    return;

                // Deleted key
                // First, check if another inherited value is still available
                for (int i = sourceIndex - 1; i >= 0; --i)
                {
                    var newSource = sources[i];
                    var internalValue = newSource.GetInternalValue(key);
                    if (internalValue != null)
                    {
                        InheritValue(internalValue, key);
                        return;
                    }
                }

                // Otherwise simply remove it
                Remove(key);
                return;
            }

            var sourceKey = key; //sourceInternalValue.Key;
            var index = GetKeyIndex(sourceKey);

            if (index != -1)
            {
                // We already have a value, check if this one is a better override
                var currentValueSourceIndex = FindOverrideGroupIndex(InternalValues.Items[index]);

                if (sourceIndex >= currentValueSourceIndex) // || currentValueSourceIndex == -1)
                {
                    InheritValue(sourceInternalValue, sourceKey);
                }
            }
            else
            {
                // New key
                InheritValue(sourceInternalValue, sourceKey);
            }
        }

        /// <summary>
        /// Updates ValueChanged event for newly changed InternalValue.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newInternalValue"></param>
        /// <param name="oldInternalValue"></param>
        internal void UpdateValueChanged(ParameterKey key, InternalValue newInternalValue, InternalValue oldInternalValue)
        {
            if (valueChangedEvents != null)
            {
                foreach (var valueChangedEvent in valueChangedEvents)
                {
                    if (valueChangedEvent.Key.Key == key || valueChangedEvent.Key.Key == null)
                    {
                        var events = valueChangedEvent.Value;
                        InternalValueChangedDelegate internalEvent;
                        if (!events.TryGetValue(key, out internalEvent))
                        {
                            var originalEvent = valueChangedEvent.Key.ValueChanged;
                            internalEvent = CreateInternalValueChangedEvent(key, internalEvent, originalEvent);
                            events.Add(key, internalEvent);
                        }
                        if (oldInternalValue != null)
                            oldInternalValue.ValueChanged -= internalEvent;
                        if (newInternalValue != null)
                            newInternalValue.ValueChanged += internalEvent;
                    }
                }
            }
        }

        private static InternalValueChangedDelegate CreateInternalValueChangedEvent(ParameterKey key, InternalValueChangedDelegate internalEvent, ValueChangedDelegate originalEvent)
        {
            internalEvent = (internalValue, oldValue) => originalEvent(key, internalValue, oldValue);
            return internalEvent;
        }

        #region Key mapping

        /// <summary>
        /// Get InternalValue at given index of key mapping specified with SetKeyMapping.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal InternalValue GetUpdatedInternalValue(int index)
        {
            return IndexedInternalValues[index];
        }

        internal T GetResource<T>(int index)
        {
            return (T)GetResource(IndexedInternalValues[index]);
        }

        /// <summary>
        /// Called when any InternalValue is updated so that key mapping is kept updated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="internalValue"></param>
        private void UpdateKeyMapping(ParameterKey key, InternalValue internalValue)
        {
            int index;

            if (keyMapping.TryGetValue(key, out index))
            {
                IndexedInternalValues[index] = internalValue;
            }
        }

        /// <summary>
        /// Sets a specific key mapping, which can then be used when querying for InternalValue with GetUpdatedInternalValue(index).
        /// It allows for skipping key lookup when performance is required (i.e. in rendering code).
        /// </summary>
        /// <param name="newKeyMapping">The key mapping.</param>
        /// <exception cref="System.ArgumentNullException">newKeyMapping</exception>
        internal void SetKeyMapping(Dictionary<ParameterKey, int> newKeyMapping)
        {
            if (newKeyMapping == null) throw new ArgumentNullException("newKeyMapping");
            this.keyMapping = newKeyMapping;
            this.IndexedInternalValues = new InternalValue[newKeyMapping.Count];
            foreach (var internalValue in InternalValues)
            {
                UpdateKeyMapping(internalValue.Key, internalValue.Value);
            }
        }

        #endregion

        /// <summary>
        /// Releases this InternalValue and its associated data.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="internalValue"></param>
        private void ReleaseValue(ParameterKey key, InternalValue internalValue)
        {
            if (internalValue == null)
                return;

            if (!key.IsValueType && internalValue.Owner == this)
            {
                internalValue.Object = null;
            }
        }

        /// <summary>
        /// Gets internal value from specificed key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal InternalValue GetInternalValue(ParameterKey key)
        {
            int index = GetKeyIndex(key);
            if (index == -1)
                return null;

            return InternalValues.Items[index].Value;
        }
        
        /// <summary>
        /// Gets or creates an internal value given its index and key.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private InternalValue GetOrCreateInternalValue(int index, ParameterKey key, out bool newValue)
        {
            var oldInternalValue = InternalValues.Items[index];
            var internalValue = oldInternalValue;
            newValue = false;

            if (internalValue.Value != null && internalValue.Value.Owner != this)
            {
                internalValue = new KeyValuePair<ParameterKey, InternalValue>(internalValue.Key, null);
            }
            if (internalValue.Value == null)
            {
                newValue = true;
                InternalValues.Items[index] = internalValue = new KeyValuePair<ParameterKey, InternalValue>(key, CreateInternalValue(key));
                internalValue.Value.Owner = this;

                OnKeyUpdate(key, internalValue.Value, oldInternalValue.Value);
            }
            return internalValue.Value;
        }

        #region Implements IParameterCollectionInheritanceInternal

        int IParameterCollectionInheritanceInternal.GetInternalValueCount()
        {
            return InternalCount;
        }

        InternalValue IParameterCollectionInheritanceInternal.GetInternalValue(ParameterKey key)
        {
            return GetInternalValue(key);
        }

        IEnumerable<KeyValuePair<ParameterKey, InternalValue>> IParameterCollectionInheritanceInternal.GetInternalValues()
        {
            return InternalValues;
        }

        ParameterCollection IParameterCollectionInheritanceInternal.GetParameterCollection()
        {
            return this;
        }

        OnUpdateValueDelegate IParameterCollectionInheritanceInternal.GetUpdateValueDelegate(ParameterCollection.OnUpdateValueDelegate original)
        {
            return original;
        }

        #endregion
        
        /// <summary>
        /// Dynamic values use this class when pointing to a source.
        /// </summary>
        internal struct InternalValueReference
        {
            public InternalValue Entry;
            public int Counter;
        }

        /// <summary>
        /// Holds a value inside ParameterCollection.
        /// </summary>
        public abstract class InternalValue
        {
            public int Counter;
            public ParameterCollection Owner;
            internal InternalValueChangedDelegate ValueChanged;
            internal InternalValueReference[] Dependencies;

            public virtual void ReadFrom(IntPtr dest, int offset, int size)
            {
                throw new NotImplementedException();
            }

            public virtual object Object
            {
                get { return null; }
                set { throw new NotImplementedException(); }
            }

            public abstract void SerializeHash(SerializationStream stream);

            /// <summary>
            /// Determines if this instance and the given internal value have same <see cref="Value"/>.
            /// </summary>
            /// <param name="internalValue">The internal value.</param>
            /// <returns></returns>
            public abstract bool ValueEquals(InternalValue internalValue);

            /// <summary>
            /// Determines if this instance and the given internal value have same <see cref="Value" />.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public abstract bool ValueEquals(object value);

            /// <summary>
            /// Determines whether [is default value] [the specified parameter key].
            /// </summary>
            /// <param name="parameterKey">The parameter key.</param>
            /// <returns></returns>
            public abstract bool IsDefaultValue(ParameterKey parameterKey);

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendFormat("({0}) {1} Count {2}", Owner.Name, Object, Counter);
                return builder.ToString();
            }
        }

        /// <summary>
        /// Holds a value of a specific type in a ParameterCollection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class InternalValueBase<T> : InternalValue
        {
            private static EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            private static DataSerializer<T> dataSerializer;

            public T Value;

            public override object Object
            {
                get { return Value; }
                set { Value = (T)value; }
            }

            public override bool IsDefaultValue(ParameterKey parameterKey)
            {
                var parameterKeyT = parameterKey as ParameterKey<T>;
                if (parameterKeyT == null)
                    return false;

                return comparer.Equals(Value, parameterKeyT.DefaultValueMetadataT.DefaultValue);
            }

            public override bool ValueEquals(InternalValue internalValue)
            {
                var internalValueT = internalValue as InternalValueBase<T>;
                if (internalValueT == null)
                    return false;

                return comparer.Equals(Value, internalValueT.Value);
            }

            public override bool ValueEquals(object value)
            {
                if (value == null && Value == null)
                    return true;

                if (!(value is T))
                    return false;

                return comparer.Equals(Value, (T)value);
            }

            public override void SerializeHash(SerializationStream stream)
            {
                var currentDataSerializer = dataSerializer;
                if (currentDataSerializer == null)
                {
                    dataSerializer = currentDataSerializer = MemberSerializer<T>.Create(stream.Context.SerializerSelector);
                }

                currentDataSerializer.Serialize(ref Value, ArchiveMode.Serialize, stream);
            }

            protected void SetValue(T value)
            {
                Value = value;
            }

            protected T GetValue()
            {
                return Value;
            }

        }

        public class InternalValue<T> : InternalValueBase<T>
        {
            public override unsafe void ReadFrom(IntPtr dest, int offset, int size)
            {
                if (offset == 0 && size == Utilities.UnsafeSizeOf<T>())
                {
                    Utilities.UnsafeWrite(dest, ref Value);
                }
                else
                {
                    Utilities.CopyMemory(dest, (IntPtr)Interop.Fixed(ref Value) + offset, size);
                }
            }

        }

        public class InternalValueArray<T> : InternalValueBase<T[]>
        {
            private int elementSize;

            public InternalValueArray(int length)
            {
                elementSize = Utilities.UnsafeSizeOf<T>();
                if (length != -1)
                {
                    // TODO Workaround for obfuscation. Cannoy set base.Value directly here
                    SetValue(new T[length]);
                }
            }

            public override unsafe void ReadFrom(IntPtr dest, int offset, int size)
            {
                if (GetValue() == null)
                    return;

                // Compute the maximum copyable size so that we don't go out of bounds.
                int maxSize = (elementSize * GetValue().Length - offset);
                if (maxSize <= 0)
                    return;
                Utilities.CopyMemory(dest, (IntPtr)Interop.Fixed(ref GetValue()[0]) + offset, Math.Min(size, maxSize));
            }
        }

        public struct KeyCollection : IReadOnlyList<ParameterKey>
        {
            private ParameterCollection parameterCollection;

            public KeyCollection(ParameterCollection parameterCollection)
            {
                this.parameterCollection = parameterCollection;
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator<ParameterKey> IEnumerable<ParameterKey>.GetEnumerator()
            {
                return GetEnumerator();
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(parameterCollection.InternalValues);
            }

            /// <inheritdoc/>
            public int Count { get { return parameterCollection.InternalValues.Count; } }

            /// <inheritdoc/>
            public ParameterKey this[int index]
            {
                get { return parameterCollection.InternalValues[index].Key; }
            }

            public struct Enumerator : IEnumerator<ParameterKey>
            {
                private FastListStruct<KeyValuePair<ParameterKey, InternalValue>> valueList;
                private int index;
                private int length;

                public Enumerator(FastListStruct<KeyValuePair<ParameterKey, InternalValue>> valueList) : this()
                {
                    this.valueList = valueList;
                    index = -1;
                    length = valueList.Count;
                }

                /// <inheritdoc/>
                public void Dispose()
                {
                }

                /// <inheritdoc/>
                public bool MoveNext()
                {
                    if (index < length)
                        return ++index < length;

                    return false;
                }

                /// <inheritdoc/>
                public void Reset()
                {
                    index = -1;
                }

                /// <inheritdoc/>
                public ParameterKey Current
                {
                    get { return valueList[index].Key; }
                }

                /// <inheritdoc/>
                object IEnumerator.Current
                {
                    get { return Current; }
                }
            }
        }

        private struct ValueChangedEventKey : IEquatable<ValueChangedEventKey>
        {
            public ParameterKey Key;
            public ValueChangedDelegate ValueChanged;

            public ValueChangedEventKey(ParameterKey key, ValueChangedDelegate valueChanged)
            {
                Key = key;
                ValueChanged = valueChanged;
            }

            public bool Equals(ValueChangedEventKey other)
            {
                return Key.Equals(other.Key) && ValueChanged.Equals(other.ValueChanged);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ValueChangedEventKey && Equals((ValueChangedEventKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Key.GetHashCode()*397) ^ ValueChanged.GetHashCode();
                }
            }
        }
    }
}