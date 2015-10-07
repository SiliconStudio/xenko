// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Extensions for asset import.
    /// </summary>
    public static class AssetImportExtensions
    {
        /// <summary>
        /// Check if there is any matching asset from a collection of <see cref="AssetToImportMerge"/>.
        /// </summary>
        /// <param name="merges"></param>
        /// <returns><c>true</c> if there is a least one matching asset</returns>
        public static bool HasMatching(this List<AssetToImportMerge> merges)
        {
            return FindBestMatching(merges) != null;
        }

        /// <summary>
        /// Returns the best matching asset from a collection of <see cref="AssetToImportMerge"/>. May be null even if the collection is != 0.
        /// </summary>
        /// <param name="merges"></param>
        /// <returns>The best matching asset or null if not found.</returns>
        public static AssetToImportMerge FindBestMatching(this List<AssetToImportMerge> merges)
        {
            return merges.FirstOrDefault(merge => merge.MatchingFactor > 0);
        }
    }
}