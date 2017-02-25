using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A container used to serialize dictionary whose entries have identifiers.
    /// </summary>
    /// <typeparam name="TKey">The type of key contained in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of value contained in the dictionary.</typeparam>
    [DataContract]
    public class DictionaryWithItemIds<TKey, TValue> : OrderedDictionary<KeyWithId<TKey>, TValue>
    {

    }
}