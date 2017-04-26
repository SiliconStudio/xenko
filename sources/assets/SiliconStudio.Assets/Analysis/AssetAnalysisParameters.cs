// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Parameters for asset analysis.
    /// </summary>
    public class AssetAnalysisParameters
    {
        public bool IsLoggingAssetNotFoundAsError { get; set; }

        public bool IsProcessingAssetReferences { get; set; }

        public bool IsProcessingUPaths { get; set; }

        public bool SetDirtyFlagOnAssetWhenFixingAbsoluteUFile { get; set; }

        public bool SetDirtyFlagOnAssetWhenFixingUFile { get; set; }

        public UPathType ConvertUPathTo { get; set; }

        public virtual AssetAnalysisParameters Clone()
        {
            return (AssetAnalysisParameters)MemberwiseClone();
        }
    }
}
