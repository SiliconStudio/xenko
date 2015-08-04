// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Provides a hook in <see cref="AssetDiff.Compute(bool)"/>.
    /// </summary>
    public interface IDiffResolver
    {
        void BeforeDiff(Asset baseAsset, Asset asset1, Asset asset2);
    }
}