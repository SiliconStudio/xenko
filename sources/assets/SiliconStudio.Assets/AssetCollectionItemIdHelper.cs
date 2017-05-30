// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    // TODO: at some point we should converge to a state where collection ids, which are for override and asset specific, should move from Core.Design to this assembly (with related yaml serializer). Meanwhile, we have to split some of the logic in an unclean manner.
    public static class AssetCollectionItemIdHelper
    {
        public static void GenerateMissingItemIds(object rootObject)
        {
            var visitor = new CollectionIdGenerator();
            visitor.Visit(rootObject);
        }
    }
}
