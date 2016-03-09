// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Class PackageAnalysisParameters. This class cannot be inherited.
    /// </summary>
    public sealed class PackageAnalysisParameters : AssetAnalysisParameters
    {
        public bool IsPackageCheckDependencies { get; set; }

        public bool AssetTemplatingMergeModifiedAssets { get; set; }

        public bool AssetTemplatingRemoveUnusedBaseParts { get; set; }
    }
}