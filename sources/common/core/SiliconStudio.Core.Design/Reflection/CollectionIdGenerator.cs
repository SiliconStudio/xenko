using System;
using System.Collections;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A visitor that will generate a <see cref="CollectionItemIdentifiers"/> collection for each collection or dictionary of the visited object,
    /// and attach it to the related collection.
    /// </summary>
    public class CollectionIdGenerator : DataVisitorBase
    {
        public override void VisitArray(Array array, ArrayDescriptor descriptor)
        {
            CollectionItemIdentifiers itemIds;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(array, out itemIds))
            {
                itemIds = CollectionItemIdHelper.GetCollectionItemIds(array);
                for (var i = 0; i < array.Length; ++i)
                {
                    itemIds.Add(i, ItemId.New());
                }
            }
            base.VisitArray(array, descriptor);
        }

        public override void VisitCollection(IEnumerable collection, CollectionDescriptor descriptor)
        {
            CollectionItemIdentifiers itemIds;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(collection, out itemIds))
            {
                itemIds = CollectionItemIdHelper.GetCollectionItemIds(collection);
                var count = descriptor.GetCollectionCount(collection);
                for (var i = 0; i < count; ++i)
                {
                    itemIds.Add(i, ItemId.New());
                }
            }
            base.VisitCollection(collection, descriptor);
        }

        public override void VisitDictionary(object dictionary, DictionaryDescriptor descriptor)
        {
            CollectionItemIdentifiers itemIds;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(dictionary, out itemIds))
            {
                itemIds = CollectionItemIdHelper.GetCollectionItemIds(dictionary);
                foreach (var element in descriptor.GetEnumerator(dictionary))
                {
                    itemIds.Add(element.Key, ItemId.New());
                }
            }
            base.VisitDictionary(dictionary, descriptor);
        }
    }
}
