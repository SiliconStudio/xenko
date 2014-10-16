// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool TryGetValueCast<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dictionary, TKey key, out TResult result)
            where TResult : TValue
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                result = (TResult)value;
                return true;
            }

            result = default(TResult);
            return false;
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }
    }
}