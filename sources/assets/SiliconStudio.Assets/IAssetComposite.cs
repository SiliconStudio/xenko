// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface that defines the composition declared by an asset inheriting from another asset.
    /// </summary>
    public interface IAssetComposite
    {
        /// <summary>
        /// Collects the part assets.
        /// </summary>
        IEnumerable<AssetPart> CollectParts();

        /// <summary>
        /// Checks if this <see cref="AssetPart"/> container contains the part with the specified id.
        /// </summary>
        /// <param name="id">Unique identifier of the asset part</param>
        /// <returns><c>true</c> if this asset contains the part with the specified id; otherwise <c>false</c></returns>
        bool ContainsPart(Guid id);
    }
}
