// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A logger that stores added and removed assets of an import operation.
    /// </summary>
    public class ImportResult : LoggerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportResult"/> class.
        /// </summary>
        public ImportResult()
        {
            RemovedAssets = new List<Guid>();
            AddedAssets = new List<AssetItem>();
            Module = "Import";
        }

        /// <summary>
        /// Gets the list of assets that have been removed from the package.
        /// </summary>
        public List<Guid> RemovedAssets { get; private set; }

        /// <summary>
        /// Gets the list of assets that have been added to the package.
        /// </summary>
        public List<AssetItem> AddedAssets { get; private set; }

        public override void Clear()
        {
            base.Clear();
            AddedAssets.Clear();
            RemovedAssets.Clear();
        }
    }
}