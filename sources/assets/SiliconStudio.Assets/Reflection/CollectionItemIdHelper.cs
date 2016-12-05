namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A helper static class to retrieve <see cref="CollectionItemIdentifiers"/> from a collection or dictionary through the <see cref="ShadowObject"/> registry.
    /// </summary>
    public static class CollectionItemIdHelper
    {
        // TODO: rework the API of this class once the feature is complete.
        public static readonly object DeletedKey = new object();

        // TODO: do we really need to pass an object to this constructor?
        public static ShadowObjectPropertyKey CollectionItemIdKey = new ShadowObjectPropertyKey(new object(), false);

        public static bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds)
        {
            var shadow = ShadowObject.Get(instance);
            if (shadow == null)
            {
                itemIds = null;
                return false;
            }

            object result;
            itemIds = shadow.TryGetValue(CollectionItemIdKey, out result) ? (CollectionItemIdentifiers)result : null;
            return result != null;
        }

        public static CollectionItemIdentifiers GetCollectionItemIds(object instance)
        {
            var shadow = ShadowObject.GetOrCreate(instance);
            object result;
            if (shadow.TryGetValue(CollectionItemIdKey, out result))
            {
                return (CollectionItemIdentifiers)result;
            }

            var itemIds = new CollectionItemIdentifiers();
            shadow.Add(CollectionItemIdKey, itemIds);
            return itemIds;
        }
    }
}
