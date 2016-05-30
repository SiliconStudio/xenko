using System;
using System.Collections.Generic;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tracking
{
    /// <summary>
    /// Describes a change related to the source files used by an asset.
    /// </summary>
    public enum SourceFileChangeType
    {
        /// <summary>
        /// The change occurred in an asset that now has a different list of source files.
        /// </summary>
        Asset,
        /// <summary>
        /// The change occurred in an source file that has been modified externally.
        /// </summary>
        SourceFile
    }

    public struct SourceFileChangedData
    {
        public SourceFileChangedData(SourceFileChangeType type, Guid assetId, IReadOnlyList<UFile> files)
        {
            Type = type;
            AssetId = assetId;
            Files = files;
        }

        public SourceFileChangeType Type { get; }

        public Guid AssetId { get; }

        public IReadOnlyList<UFile> Files { get; }
    }
}
