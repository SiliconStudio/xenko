using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A base implementation of the <see cref="IAssetFactory{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of asset this factory can create.</typeparam>
    public abstract class AssetFactory<T> : IAssetFactory<T> where T : Asset
    {
        /// <inheritdoc/>
        public Type AssetType => typeof(T);

        /// <inheritdoc/>
        public abstract T New();
    }
}