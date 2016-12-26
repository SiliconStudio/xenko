using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
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

    public class AssetPartCollectionSerializer<TAssetPartDesign, TAssetPart> : KeyedSortedListSerializer<AssetPartCollection<TAssetPartDesign, TAssetPart>, Guid, TAssetPartDesign>
        where TAssetPartDesign : IAssetPartDesign<TAssetPart>
        where TAssetPart : IIdentifiable
    {
    }
}
