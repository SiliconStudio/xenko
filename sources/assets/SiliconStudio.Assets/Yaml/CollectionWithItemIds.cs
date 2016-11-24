using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A container used to serialize collection whose items have identifiers.
    /// </summary>
    /// <typeparam name="TItem">The type of item contained in the collection.</typeparam>
    [DataContract]
    public class CollectionWithItemIds<TItem> : OrderedDictionary<ItemId, TItem>
    {
    }
}
