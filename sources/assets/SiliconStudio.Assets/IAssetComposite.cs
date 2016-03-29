// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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


        /// <summary>
        /// Overwrites the baseId and basePartInstanceId for the specified part given its id.
        /// </summary>
        /// <param name="id">The id of the part to modify</param>
        /// <param name="baseId">The new baseId</param>
        /// <param name="basePartInstanceId">The new basePartInstanceId</param>
        void SetPart(Guid id, Guid baseId, Guid basePartInstanceId);
    }
}