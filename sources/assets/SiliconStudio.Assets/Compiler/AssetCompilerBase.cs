// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

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

            var result = new AssetCompilerResult(GetType().Name)
            {
                BuildSteps = new AssetBuildStep(assetItem)
            };

            // Only use the path to the asset without its extension
            var fullPath = assetItem.FullPath;
            if (!fullPath.IsAbsolute)
            {
                throw new InvalidOperationException("assetItem must be an absolute path");
            }

            Compile((AssetCompilerContext)context, assetItem.Location.GetDirectoryAndFileName(), assetItem.FullPath, (T)assetItem.Asset, result);

            AssetItem = null;

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

        /// <summary>
        /// Returns the absolute path on the disk of an <see cref="UFile"/> that is relative to the asset location.
        /// </summary>
        /// <param name="assetAbsolutePath">The absolute path of the asset on the disk.</param>
        /// <param name="relativePath">The path relative to the asset path that must be converted to an absolute path.</param>
        /// <returns>The absolute path on the disk of the <see cref="relativePath"/> argument.</returns>
        /// <exception cref="ArgumentException">The <see cref="relativePath"/> argument is a null or empty <see cref="UFile"/>.</exception>
        protected static UFile GetAbsolutePath(UFile assetAbsolutePath, UFile relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentException("The relativePath argument is null or empty");
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, relativePath);
            return assetSource;
        }

        /// <summary>
        /// Ensures that the source of an <see cref="AssetImport"/> exists. If the given asset is not an <see cref="AssetImport"/>, this method simply returns <c>true</c>.
        /// </summary>
        /// <param name="result">The <see cref="AssetCompilerResult"/> in which to output log of potential errors.</param>
        /// <param name="assetImport">The asset to check.</param>
        /// <param name="assetAbsolutePath">The absolute path of the asset on the disk</param>
        /// <returns><c>true</c> if the source file exists, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Any of the argument is <c>null</c>.</exception>
        protected static bool EnsureSourceExists(AssetCompilerResult result, T assetImport, UFile assetAbsolutePath)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (assetImport == null) throw new ArgumentNullException("assetImport");
            if (assetAbsolutePath == null) throw new ArgumentNullException("assetAbsolutePath");

            var asset = assetImport as AssetImport;

            // The asset has no source, so there is no failure
            if (asset == null)
                return true;

            if (string.IsNullOrEmpty(asset.Source))
            {
                result.Error("Source is null for Asset [{0}]", asset);
                return false;
            }

            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            // Ensure the file exists
            if (!File.Exists(assetSource))
            {
                result.Error("Unable to find the source file '{1}' for Asset [{0}]", asset, assetSource);
                return false;
            }

            return true;
        }
    }
}