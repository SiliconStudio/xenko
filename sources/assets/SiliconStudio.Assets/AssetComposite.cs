// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Base class for an asset that supports inheritance by composition.
    /// </summary>
    public abstract class AssetComposite : Asset, IAssetComposite
    {
        /// <summary>
        /// Adds the given <see cref="AssetBase"/> to the <see cref="Asset.BaseParts"/> collection of this asset.
        /// </summary>
        /// <remarks>If the <see cref="Asset.BaseParts"/> collection already contains the argument. this method does nothing.</remarks>
        /// <param name="newBasePart">The base to add to the <see cref="Asset.BaseParts"/> collection.</param>
        public void AddBasePart(AssetBase newBasePart)
        {
            if (newBasePart == null) throw new ArgumentNullException(nameof(newBasePart));

            if (BaseParts == null)
            {
                BaseParts = new List<AssetBase>();
            }

            if (BaseParts.All(x => x.Id != newBasePart.Id))
            {
                BaseParts.Add(newBasePart);
            }
        }

        public abstract IEnumerable<AssetPart> CollectParts();

        public abstract void SetPart(Guid id, Guid baseId, Guid basePartInstanceId);

        public abstract bool ContainsPart(Guid id);

        public abstract void FixupPartReferences();
    }
}
