// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Base implementation for <see cref="IAssetCompiler"/> suitable to compile a single type of <see cref="Asset"/>.
    /// </summary>
    /// <typeparam name="T">Type of the asset</typeparam>
    public abstract class AssetCompilerBase<T> : IAssetCompiler where T : Asset
    {
        /// <summary>
        /// The current <see cref="AssetItem"/> to compile.
        /// </summary>
        protected AssetItem AssetItem;

        public AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (assetItem == null) throw new ArgumentNullException("assetItem");

            AssetItem = assetItem;

            var result = new AssetCompilerResult(GetType().Name);

            // Only use the path to the asset without its extension
            var fullPath = assetItem.FullPath;
            if (!fullPath.IsAbsolute)
            {
                throw new InvalidOperationException("assetItem must be an absolute path");
            }

            Compile((AssetCompilerContext)context, assetItem.Location.GetDirectoryAndFileName(), assetItem.FullPath, (T)assetItem.Asset, result);
            return result;
        }

        /// <summary>
        /// Compiles the asset from the specified package.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="urlInStorage">The absolute URL to the asset, relative to the storage.</param>
        /// <param name="assetAbsolutePath">Absolute path of the asset on the disk</param>
        /// <param name="asset">The asset.</param>
        /// <param name="result">The result where the commands and logs should be output.</param>
        protected abstract void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, T asset, AssetCompilerResult result);
    }
}