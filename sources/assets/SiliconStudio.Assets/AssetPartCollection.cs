using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
    [DataSerializer(typeof(AssetPartCollectionSerializer<>), Mode = DataSerializerGenericMode.GenericArguments)]
    public class AssetPartCollection<TAssetPart> : KeyedSortedList<Guid, TAssetPart> where TAssetPart : IIdentifiable
    {
        protected override Guid GetKeyForItem(TAssetPart item)
        {
            return item.Id;
        }

        public void AddRange(IEnumerable<TAssetPart> partDesigns)
        {
            foreach (var partDesign in partDesigns)
            {
                Add(partDesign);
            }
        }
    }

    [DataSerializer(typeof(AssetPartCollectionSerializer<,>), Mode = DataSerializerGenericMode.GenericArguments)]
    public class AssetPartCollection<TAssetPartDesign, TAssetPart> : KeyedSortedList<Guid, TAssetPartDesign>
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : IIdentifiable
    {
        protected override Guid GetKeyForItem([NotNull] TAssetPartDesign item)
        {
            return item.Part.Id;
        }

        public void AddRange([ItemNotNull, NotNull]  IEnumerable<TAssetPartDesign> partDesigns)
        {
            foreach (var partDesign in partDesigns)
            {
                Add(partDesign);
            }
        }
    }

    public class AssetPartCollectionSerializer<TAssetPart> : KeyedSortedListSerializer<AssetPartCollection<TAssetPart>, Guid, TAssetPart>
    where TAssetPart : IIdentifiable
    {
    }

    public class AssetPartCollectionSerializer<TAssetPartDesign, TAssetPart> : KeyedSortedListSerializer<AssetPartCollection<TAssetPartDesign, TAssetPart>, Guid, TAssetPartDesign>
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : IIdentifiable
    {
    }
}
