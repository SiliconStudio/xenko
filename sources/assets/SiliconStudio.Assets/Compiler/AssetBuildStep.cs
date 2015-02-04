using System;

using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Represents a list of <see cref="BuildStep"/> instances that compiles a given asset.
    /// </summary>
    public class AssetBuildStep : ListBuildStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBuildStep"/> class.
        /// </summary>
        /// <param name="assetItem">The asset that can be build by this build step.</param>
        public AssetBuildStep(AssetItem assetItem)
        {
            if (assetItem == null) throw new ArgumentNullException("assetItem");
            AssetItem = assetItem;
        }

        /// <summary>
        /// Gets the <see cref="AssetItem"/> corresponding to the asset being built by this build step.
        /// </summary>
        public AssetItem AssetItem { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("Asset build steps [{0}:'{1}'] ({2} items)", AssetItem.Asset != null ? AssetItem.Asset.GetType().Name : "(null)", AssetItem.Location, Count);
        }
    }
}