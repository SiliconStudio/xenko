using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface that represents an asset factory.
    /// </summary>
    /// <typeparam name="T">The type of asset this factory can create.</typeparam>
    public interface IAssetFactory<out T> where T : Asset
    {
        /// <summary>
        /// Retrieve the asset type associated to this factory.
        /// </summary>
        /// <returns>The asset type associated to this factory.</returns>
        Type AssetType { get; }

        /// <summary>
        /// Creates a new instance of the asset type associated to this factory.
        /// </summary>
        /// <returns>A new instance of the asset type associated to this factory.</returns>
        T New();
    }
}