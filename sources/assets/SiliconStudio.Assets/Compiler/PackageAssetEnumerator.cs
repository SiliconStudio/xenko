// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Enumerate all assets of this package and its references.
    /// </summary>
    public class PackageAssetEnumerator : IPackageCompilerSource
    {
        private readonly Package package;

        public PackageAssetEnumerator(Package package)
        {
            this.package = package;
        }

        /// <inheritdoc/>
        public IEnumerable<AssetItem> GetAssets(AssetCompilerResult assetCompilerResult)
        {
            // Check integrity of the packages
            var packageAnalysis = new PackageSessionAnalysis(package.Session, new PackageAnalysisParameters());
            packageAnalysis.Run(assetCompilerResult);
            if (assetCompilerResult.HasErrors)
            {
                yield break;
            }

            var packages = package.GetPackagesWithRecursiveDependencies();
            foreach (var pack in packages)
            {
                foreach (var asset in pack.Assets)
                {
                    yield return asset;
                }
            }
        }
    }
}
