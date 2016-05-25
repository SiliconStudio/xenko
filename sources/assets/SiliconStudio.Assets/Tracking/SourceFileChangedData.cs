using System;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tracking
{
    public struct SourceFileChangedData
    {
        public SourceFileChangedData(Guid assetId, UFile file, bool updateAssetIfChanged)
        {
            AssetId = assetId;
            File = file;
            UpdateAssetIfChanged = updateAssetIfChanged;
        }

        public Guid AssetId { get; }

        public UFile File { get; }

        public bool UpdateAssetIfChanged { get; }
    }
}
