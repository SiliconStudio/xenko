// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Class PackageAnalysisParameters. This class cannot be inherited.
    /// </summary>
    public sealed class PackageAnalysisParameters : AssetAnalysisParameters
    {
        public bool IsPackageCheckDependencies { get; set; }
    }
}
