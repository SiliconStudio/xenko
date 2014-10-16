// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Serialization.Converters
{
    public class DictionaryDataConverter<TData, T, TDataKey, TDataValue, TKey, TValue> : DataConverter<TData, T> where TData : class, IDictionary<TDataKey, TDataValue> where T : class, IDictionary<TKey, TValue>
    {
        public override void ConvertToData(ConverterContext converterContext, ref TData data, T obj)
        {
            if (obj == null)
            {
                data = null;
                return;
            }

            if (data == null)
            {
                // Optimize most common cases
                if (typeof(TData) == typeof(Dictionary<TDataKey, TDataValue>) || typeof(TData) == typeof(IDictionary<TDataKey, TDataValue>))
                    data = (TData)(IDictionary<TDataKey, TDataValue>)new Dictionary<TDataKey, TDataValue>(obj.Count);
                else
                    data = Activator.CreateInstance<TData>();
            }
            else
            {
                data.Clear();
            }

            foreach (var item in obj)
            {
                var itemData1 = default(TDataKey);
                var itemData2 = default(TDataValue);
                converterContext.ConvertToData(ref itemData1, item.Key);
                converterContext.ConvertToData(ref itemData2, item.Value);
                data.Add(itemData1, itemData2);
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, TData data, ref T source)
        {
            if (data == null)
            {
                source = null;
                return;
            }

            if (source == null)
            {
                // Optimize most common cases
                if (typeof(T) == typeof(Dictionary<TKey, TValue>) || typeof(T) == typeof(IDictionary<TKey, TValue>))
                    source = (T)(IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>(data.Count);
                else
                    source = Activator.CreateInstance<T>();
            }
            else
            {
                source.Clear();
            }

            foreach (var item in data)
            {
                var itemData1 = default(TKey);
                var itemData2 = default(TValue);
                converterContext.ConvertFromData(item.Key, ref itemData1);
                converterContext.ConvertFromData(item.Value, ref itemData2);
                source.Add(itemData1, itemData2);
            }
        }
    }
}