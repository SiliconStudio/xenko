// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Provides a hook to do various changes after the actual merge in <see cref="AssetMerge.Merge(AssetDiff, AssetMerge.MergePolicyDelegate, bool)"/>, when previewOnly is false.
    /// </summary>
    public interface IDiffProxy
    {
        /// <summary>
        /// Apply the diff changes back to source object.
        /// </summary>
        void ApplyChanges();
    }
}