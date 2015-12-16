// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Result of a merge. Contains <see cref="Asset"/> != null if there are no errors.
    /// </summary>
    public class MergeResult : LoggerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeResult"/> class.
        /// </summary>
        public MergeResult() : base("Merge")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeResult"/> class.
        /// </summary>
        /// <param name="asset">The asset.</param>
        public MergeResult(object asset) : this()
        {
            Asset = asset;
        }

        /// <summary>
        /// Gets or sets the merged asset. This is <c>null</c> when this instance <see cref="LoggerResult.HasErrors"/> to true.
        /// </summary>
        /// <value>The asset.</value>
        public object Asset { get; set; }
    }
}