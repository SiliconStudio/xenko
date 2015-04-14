using System.Collections.Generic;

namespace SiliconStudio.Presentation.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> thisObject, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                thisObject.Add(keyValuePair);
            }
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> thisObject, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
            {
                thisObject[keyValuePair.Key] = keyValuePair.Value;
            }
        }
    }
}
