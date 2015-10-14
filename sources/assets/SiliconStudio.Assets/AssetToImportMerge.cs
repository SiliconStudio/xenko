// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Assets.Diff;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes a mergeable previous item. The item as a <see cref="MatchingFactor"/> and <see cref="IsMergeable"/>
    /// is <c>true</c> if the <see cref="AssetToImportMergeGroup.Item"/> can be merged into <see cref="PreviousItem"/>.
    /// </summary>
    [DebuggerDisplay("MergeItem: {PreviousItem} Matching: {MatchingFactor}")]
    public class AssetToImportMerge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetToImportMerge"/> class.
        /// </summary>
        /// <param name="previousItem">The previous item.</param>
        /// <param name="diff">The difference.</param>
        /// <param name="mergePreviewResult">The merge preview result.</param>
        internal AssetToImportMerge(AssetItem previousItem, AssetDiff diff, MergeResult mergePreviewResult)
        {
            PreviousItem = previousItem;
            this.Diff = diff;
            this.MergePreviewResult = mergePreviewResult;
            DependencyGroups = new List<AssetToImportMergeGroup>();
        }

        /// <summary>
        /// Gets the previous item matching the new item to import.
        /// </summary>
        /// <value>The previous item.</value>
        public AssetItem PreviousItem { get; private set; }

        /// <summary>
        /// Gets the difference between the <see cref="PreviousItem"/> and the item to import.
        /// </summary>
        /// <value>The difference.</value>
        public AssetDiff Diff { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the new item is mergeable into the <see cref="PreviousItem"/>.
        /// </summary>
        /// <value><c>true</c> if the new item is mergeable into the <see cref="PreviousItem"/>; otherwise, <c>false</c>.</value>
        public bool IsMergeable
        {
            get
            {
                return MergePreviewResult != null && !MergePreviewResult.HasErrors;
            }
        }

        /// <summary>
        /// Gets the merge preview result.
        /// </summary>
        /// <value>The merge preview result.</value>
        public MergeResult MergePreviewResult { get; private set; }

        /// <summary>
        /// Gets the matching factor, from negative to positive value. The higher the value is, the higher assets are matching
        /// </summary>
        /// <value>The matching factor.</value>
        public double MatchingFactor { get; internal set; }

        /// <summary>
        /// Gets the list of <see cref="AssetToImportMergeGroup"/> this merge is dependent on.
        /// </summary>
        /// <value>The dependency groups.</value>
        public List<AssetToImportMergeGroup> DependencyGroups { get; private set; }
    }
}