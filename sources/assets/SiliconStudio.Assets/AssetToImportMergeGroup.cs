// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes an asset to import associated with possible existing assets, mergeable or not.
    /// </summary>
    [DebuggerDisplay("Item: {Item} Merges: [{Merges.Count}]")]
    public class AssetToImportMergeGroup
    {
        internal AssetToImportMergeGroup(AssetToImportByImporter parent, AssetItem item)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (item == null) throw new ArgumentNullException("item");
            this.Parent = parent;
            Item = item;
            Merges = new List<AssetToImportMerge>();
            Enabled = true;
            var assetDescription = DisplayAttribute.GetDisplay(item.Asset.GetType());
            Log = new LoggerResult(string.Format("Import {0} {1}", assetDescription != null ? assetDescription.Name : "Asset" , item));
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public AssetToImportByImporter Parent { get; private set; }

        /// <summary>
        /// Gets the item to import.
        /// </summary>
        /// <value>The item.</value>
        public AssetItem Item { get; private set; }

        /// <summary>
        /// Gets a list of equivalent assets that could be merged into.
        /// </summary>
        /// <value>The merges.</value>
        public List<AssetToImportMerge> Merges { get; private set; }

        /// <summary>
        /// Gets or sets the selected item. If this value is set 
        /// </summary>
        /// <value>The selected item.</value>
        public AssetItem SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the final item to import.
        /// </summary>
        /// <value>The final item.</value>
        public AssetItem MergedItem { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="MergedItem"/> is a merged item.
        /// </summary>
        /// <value><c>true</c> if this instance is merged; otherwise, <c>false</c>.</value>
        public bool IsMerged
        {
            get
            {
                return MergedResult != null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AssetToImportMergeGroup"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>The log.</value>
        public LoggerResult Log { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public bool HasErrors
        {
            get
            {
                return Log.HasErrors;
            }
        }

        /// <summary>
        /// Gets the merge result.
        /// </summary>
        /// <value>The merge result.</value>
        public MergeResult MergedResult { get; internal set; }
    }
}