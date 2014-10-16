// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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