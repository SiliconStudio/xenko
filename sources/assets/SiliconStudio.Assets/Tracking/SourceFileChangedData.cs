using System;
using System.Collections.Generic;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tracking
{
    /// <summary>
    /// Data structure for the <see cref="AssetSourceTracker.SourceFileChanged"/> block.
    /// </summary>
    public struct SourceFileChangedData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFileChangedData"/> structure.
        /// </summary>
        /// <param name="type">The type of change that occurred.</param>
        /// <param name="assetId">The id of the asset affected by this change.</param>
        /// <param name="files">The list of files that changed.</param>
        /// <param name="needUpdate">Indicate whether the asset needs to be updated from its sources due to this change.</param>
        public SourceFileChangedData(SourceFileChangeType type, Guid assetId, IReadOnlyList<UFile> files, bool needUpdate)
        {
            Type = type;
            AssetId = assetId;
            Files = files;
            NeedUpdate = needUpdate;
        }

        /// <summary>
        /// Gets the type of change that occurred.
        /// </summary>
        public SourceFileChangeType Type { get; }

        /// <summary>
        /// Gets the id of the asset affected by this change.
        /// </summary>
        public Guid AssetId { get; }

        /// <summary>
        /// Gets the list of files that changed
        /// </summary>
        public IReadOnlyList<UFile> Files { get; }

        /// <summary>
        /// Gets whether the asset needs to be updated from its sources due to this change.
        /// </summary>
        public bool NeedUpdate { get; }
    }
}
