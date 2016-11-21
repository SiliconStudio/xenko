using System;
using System.Collections;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A visitor that will generate a <see cref="CollectionItemIdentifiers"/> collection for each collection or dictionary of the visited object,
    /// and attach it to the related collection.
    /// </summary>
    public class CollectionIdGenerator : DataVisitorBase
    {
        private int inNonIdentifiableType;

        protected override bool CanVisit(object obj)
        {
            return !AssetRegistry.IsContentType(obj?.GetType()) && base.CanVisit(obj);
        }

        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            var localInNonIdentifiableType = false;
            try
            {
                if (descriptor.Attributes.OfType<NonIdentifiableCollectionItemsAttribute>().Any())
                {
                    localInNonIdentifiableType = true;
                    inNonIdentifiableType++;
                }
                base.VisitObject(obj, descriptor, visitMembers);
            }
            finally
            {
                if (localInNonIdentifiableType)
                    inNonIdentifiableType--;
            }
        }
               

        public override void VisitArray(Array array, ArrayDescriptor descriptor)
        {
            CollectionItemIdentifiers itemIds;
            if (inNonIdentifiableType == 0 && !CollectionItemIdHelper.TryGetCollectionItemIds(array, out itemIds))
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
            if (inNonIdentifiableType == 0 && !CollectionItemIdHelper.TryGetCollectionItemIds(collection, out itemIds))
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
            if (inNonIdentifiableType == 0 && !CollectionItemIdHelper.TryGetCollectionItemIds(dictionary, out itemIds))
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
