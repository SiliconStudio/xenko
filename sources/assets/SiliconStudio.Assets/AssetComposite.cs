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
        /// Adds an asset composition as a part of this asset. This method has to be used in sub-classes.
        /// </summary>
        /// <param name="assetPartBase">The entity asset to be used as a part (must be created directly from <see cref="Asset.CreateChildAsset"/>)</param>
        protected void AddPartCore(AssetComposite assetPartBase)
        {
            if (assetPartBase == null) throw new ArgumentNullException(nameof(assetPartBase));

            // The assetPartBase must be a plain child asset
            if (assetPartBase.Base == null) throw new InvalidOperationException($"Expecting a Base for {nameof(assetPartBase)}");
            if (assetPartBase.BaseParts != null) throw new InvalidOperationException($"Expecting a null BaseParts for {nameof(assetPartBase)}");

            // Check that the assetPartBase contains only entities from its base (no new entity, must be a plain ChildAsset)
            if (assetPartBase.CollectParts().Any(it => !it.BaseId.HasValue))
            {
                throw new InvalidOperationException("An asset part base must contain only base assets");
            }

            // The instance id will be the id of the assetPartBase
            if (this.BaseParts == null)
            {
                this.BaseParts = new List<AssetBase>();
            }

            var basePart = this.BaseParts.FirstOrDefault(basePartIt => basePartIt.Id == assetPartBase.Base.Id);
            if (basePart == null)
            {
                basePart = assetPartBase.Base;
                this.BaseParts.Add(basePart);
            }
        }

        public abstract IEnumerable<AssetPart> CollectParts();

        public abstract bool ContainsPart(Guid id);
    }
}