// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Serialization.Converters
{
    public class ListDataConverter<TData, T, TDataItem, TItem> : DataConverter<TData, T> where TData : class, IList<TDataItem> where T : class, IList<TItem>
    {
        public override void ConvertToData(ConverterContext converterContext, ref TData data, T obj)
        {
            if (obj == null)
            {
                data = null;
                return;
            }

            bool isArray = typeof(TData) == typeof(TDataItem[]);

            if (data == null)
            {
                // Optimize most common cases
                if (typeof(TData) == typeof(List<TDataItem>) || typeof(TData) == typeof(IList<TDataItem>))
                    data = (TData)(IList<TDataItem>)new List<TDataItem>(obj.Count);
                else if (isArray)
                    data = (TData)(IList<TDataItem>)new TDataItem[obj.Count];
                else
                    data = Activator.CreateInstance<TData>();
            }
            else if (!isArray)
            {
                data.Clear();
            }

            int index = 0;
            foreach (var item in obj)
            {
                var itemData = default(TDataItem);
                converterContext.ConvertToData(ref itemData, item);
                if (isArray)
                    data[index] = itemData;
                else
                    data.Add(itemData);
                index++;
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, TData data, ref T source)
        {
            if (data == null)
            {
                source = null;
                return;
            }

            bool isArray = typeof(T) == typeof(TItem[]);

            if (source == null)
            {
                // Optimize most common cases
                if (typeof(T) == typeof(List<TItem>) || typeof(T) == typeof(IList<TItem>))
                    source = (T)(IList<TItem>)new List<TItem>(data.Count);
                else if (isArray)
                    source = (T)(IList<TItem>)new TItem[data.Count];
                else
                    source = Activator.CreateInstance<T>();
            }
            else if (!isArray)
            {
                source.Clear();
            }

            if (isArray)
            {
                var sourceArray = (TItem[])(object)source;
                for (int i = 0; i < source.Count; ++i)
                {
                    var itemData = default(TItem);
                    converterContext.ConvertFromData(data[i], ref itemData);
                    sourceArray[i] = itemData;
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var itemData = default(TItem);
                    converterContext.ConvertFromData(item, ref itemData);
                    source.Add(itemData);
                }
            }
        }
    }
}