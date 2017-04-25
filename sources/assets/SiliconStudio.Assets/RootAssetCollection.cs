// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Assets
{
    [DataSerializer(typeof(KeyedSortedListSerializer<RootAssetCollection, AssetId, AssetReference>))]
    public class RootAssetCollection : KeyedSortedList<AssetId, AssetReference>
    {
        /// <inheritdoc/>
        protected override AssetId GetKeyForItem(AssetReference item)
        {
            return item.Id;
        }
    }
}
